using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Background service that processes model training jobs.
/// Polls for queued jobs and executes Python training script.
/// </summary>
public class ModelTrainingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ModelTrainingBackgroundService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public ModelTrainingBackgroundService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ModelTrainingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Model Training Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuedJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing training jobs");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("Model Training Background Service stopped");
    }

    private async Task ProcessQueuedJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IModelTrainingJobRepository>();

        var queuedJob = await jobRepository.GetAll()
            .Where(j => j.Status == "Queued")
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(stoppingToken);

        if (queuedJob == null)
        {
            return; // No queued jobs
        }

        _logger.LogInformation("Processing training job {JobId}", queuedJob.Id);

        await ExecuteTrainingJobAsync(queuedJob, stoppingToken);
    }

    private async Task ExecuteTrainingJobAsync(ModelTrainingJob job, CancellationToken stoppingToken)
    {
        try
        {
            // Get configuration
            var pythonPath = _configuration["ModelTraining:PythonPath"] ?? "python";
            var scriptPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "scripts", "ml-training", "train_model.py"
            );
            
            // Normalize path for cross-platform compatibility
            scriptPath = Path.GetFullPath(scriptPath);
            
            if (!File.Exists(scriptPath))
            {
                _logger.LogError("Training script not found: {ScriptPath}", scriptPath);
                await MarkJobAsFailedAsync(job.Id, $"Training script not found: {scriptPath}");
                return;
            }

            // Get storage and output paths from configuration
            var storagePath = _configuration["FileStorage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
            var outputPath = _configuration["ModelTraining:OutputPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "models");
            
            // Ensure output directory exists
            Directory.CreateDirectory(outputPath);

            // Get database connection string
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Prepare Python command
            var arguments = $"\"{scriptPath}\" " +
                          $"--job-id \"{job.Id}\" " +
                          $"--connection-string \"{connectionString}\" " +
                          $"--storage-path \"{storagePath}\" " +
                          $"--output-path \"{outputPath}\"";

            _logger.LogInformation("Executing training script: {PythonPath} {Arguments}", pythonPath, arguments);

            // Execute Python script
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var outputLines = new List<string>();
            var errorLines = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputLines.Add(e.Data);
                    _logger.LogInformation("[Training] {Output}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorLines.Add(e.Data);
                    _logger.LogWarning("[Training Error] {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for process to complete (with timeout)
            var completed = await Task.Run(() => 
                process.WaitForExit((int)TimeSpan.FromHours(2).TotalMilliseconds), 
                stoppingToken);

            if (!completed)
            {
                _logger.LogError("Training process timed out after 2 hours");
                process.Kill();
                await MarkJobAsFailedAsync(job.Id, "Training timed out after 2 hours");
                return;
            }

            if (process.ExitCode != 0)
            {
                var errorMessage = string.Join("\n", errorLines.TakeLast(5));
                _logger.LogError("Training process failed with exit code {ExitCode}", process.ExitCode);
                await MarkJobAsFailedAsync(job.Id, $"Training failed: {errorMessage}");
                return;
            }

            _logger.LogInformation("Training job {JobId} completed successfully", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during training job execution");
            await MarkJobAsFailedAsync(job.Id, $"Exception: {ex.Message}");
        }
    }

    private async Task MarkJobAsFailedAsync(Guid jobId, string errorMessage)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IModelTrainingJobRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var job = await jobRepository.GetByIdAsync(jobId);
            if (job != null)
            {
                job.Status = "Failed";
                job.CompletedAt = DateTime.UtcNow;
                job.ErrorMessage = errorMessage;
                
                if (job.StartedAt.HasValue)
                {
                    job.DurationSeconds = (int)(DateTime.UtcNow - job.StartedAt.Value).TotalSeconds;
                }

                await jobRepository.UpdateAsync(job);
                await unitOfWork.SaveChangesAsync();

                _logger.LogWarning("Marked job {JobId} as failed: {Error}", jobId, errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark job as failed");
        }
    }
}
