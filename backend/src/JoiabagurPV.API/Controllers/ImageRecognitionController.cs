using JoiabagurPV.Application.DTOs.ImageRecognition;
using JoiabagurPV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for image recognition and ML model management operations.
/// </summary>
[ApiController]
[Route("api/image-recognition")]
[Authorize]
public class ImageRecognitionController : ControllerBase
{
    private readonly IImageRecognitionService _imageRecognitionService;
    private readonly IModelHealthService _modelHealthService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ImageRecognitionController> _logger;

    public ImageRecognitionController(
        IImageRecognitionService imageRecognitionService,
        IModelHealthService modelHealthService,
        ICurrentUserService currentUserService,
        ILogger<ImageRecognitionController> logger)
    {
        _imageRecognitionService = imageRecognitionService;
        _modelHealthService = modelHealthService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the active model metadata.
    /// Used by frontend to check model version and decide whether to download.
    /// </summary>
    /// <returns>Model metadata if active model exists.</returns>
    [HttpGet("model/metadata")]
    [ProducesResponseType(typeof(ModelMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelMetadataDto>> GetModelMetadata()
    {
        var metadata = await _imageRecognitionService.GetActiveModelMetadataAsync();

        if (metadata == null)
        {
            return NotFound(new { message = "No AI model available yet. Please train an initial model." });
        }

        return Ok(metadata);
    }

    /// <summary>
    /// Gets model health metrics and alert level (admin only).
    /// Used by admin dashboard to display model status and determine if retraining is needed.
    /// </summary>
    /// <returns>Model health metrics with alert level.</returns>
    [HttpGet("model/health")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ModelHealthDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ModelHealthDto>> GetModelHealth()
    {
        var health = await _modelHealthService.GetModelHealthAsync();
        return Ok(health);
    }

    /// <summary>
    /// Gets all model versions (training history).
    /// </summary>
    /// <returns>List of all model versions ordered by training date.</returns>
    [HttpGet("model/versions")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(List<ModelMetadataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ModelMetadataDto>>> GetModelVersions()
    {
        var versions = await _imageRecognitionService.GetAllModelVersionsAsync();
        return Ok(versions);
    }

    /// <summary>
    /// Initiates model retraining (admin only).
    /// Creates an async job and returns immediately with job ID.
    /// </summary>
    /// <returns>Training job ID for status polling.</returns>
    [HttpPost("retrain")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InitiateRetraining()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var jobId = await _imageRecognitionService.InitiateModelRetrainingAsync(_currentUserService.UserId.Value);

            _logger.LogInformation(
                "Model retraining initiated by user {UserId}, job {JobId}",
                _currentUserService.UserId.Value, jobId);

            return Accepted(new
            {
                jobId,
                message = "Model retraining job created successfully.",
                statusUrl = $"/api/image-recognition/retrain/status/{jobId}",
                estimatedDuration = "30-45 minutes"
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already in progress"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of a training job.
    /// </summary>
    /// <param name="jobId">Training job ID.</param>
    /// <returns>Job status with progress information.</returns>
    [HttpGet("retrain/status/{jobId}")]
    [ProducesResponseType(typeof(TrainingJobStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrainingJobStatusDto>> GetTrainingStatus(Guid jobId)
    {
        var status = await _imageRecognitionService.GetTrainingJobStatusAsync(jobId);

        if (status == null)
        {
            return NotFound(new { message = "Training job not found." });
        }

        return Ok(status);
    }

    /// <summary>
    /// Gets the latest training job status.
    /// </summary>
    /// <returns>Latest job status if exists.</returns>
    [HttpGet("retrain/latest")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TrainingJobStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrainingJobStatusDto>> GetLatestTrainingStatus()
    {
        var status = await _imageRecognitionService.GetLatestTrainingJobAsync();

        if (status == null)
        {
            return NotFound(new { message = "No training jobs found." });
        }

        return Ok(status);
    }

    /// <summary>
    /// Downloads the active model files (TensorFlow.js format).
    /// Returns the model directory path for frontend to download model.json and .bin files.
    /// </summary>
    /// <returns>Model file path if available.</returns>
    [HttpGet("model")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModel()
    {
        var modelPath = await _imageRecognitionService.GetModelFilePathAsync();

        if (string.IsNullOrEmpty(modelPath))
        {
            return NotFound(new { message = "No model available. Please train a model first." });
        }

        // Return model path for frontend to construct download URLs
        // Frontend will fetch model.json and .bin files separately
        // Get metadata for additional info
        var metadata = await _imageRecognitionService.GetActiveModelMetadataAsync();
        
        return Ok(new
        {
            modelPath,
            version = metadata?.Version,
            trainedAt = metadata?.TrainedAt,
            modelJsonUrl = $"/api/image-recognition/model/files/{metadata?.Version}/model.json",
            message = "Model available for download"
        });
    }

    /// <summary>
    /// Downloads a specific model file (model.json or .bin files).
    /// </summary>
    /// <param name="version">Model version.</param>
    /// <param name="fileName">File name (e.g., model.json, group1-shard1of1.bin).</param>
    /// <returns>Model file content.</returns>
    [HttpGet("model/files/{version}/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModelFile(string version, string fileName)
    {
        // Validate file name to prevent directory traversal
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest(new { message = "Invalid file name." });
        }

        // Verify model version exists
        var modelPath = await _imageRecognitionService.GetModelFilePathAsync(version);
        if (string.IsNullOrEmpty(modelPath))
        {
            return NotFound(new { message = "Model version not found." });
        }

        // Construct full file path
        var modelsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "models");
        var filePath = Path.Combine(modelsBasePath, version, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Model file not found: {FilePath}", filePath);
            return NotFound(new { message = "Model file not found." });
        }

        // Determine content type
        var contentType = fileName.EndsWith(".json") ? "application/json" : "application/octet-stream";

        // Return file with caching headers
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        Response.Headers.Append("Cache-Control", "public, max-age=86400"); // Cache for 24 hours
        
        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// Uploads a browser-trained model (admin only).
    /// Receives model.json and weight files from browser-based TensorFlow.js training.
    /// </summary>
    /// <returns>Upload result with new model metadata.</returns>
    [HttpPost("upload-trained-model")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UploadTrainedModelResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(100_000_000)] // 100MB limit for model files
    public async Task<ActionResult<UploadTrainedModelResult>> UploadTrainedModel([FromForm] UploadTrainedModelFormRequest request)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        // Validate request
        if (string.IsNullOrEmpty(request.Version))
        {
            return BadRequest(new { message = "Model version is required." });
        }

        if (string.IsNullOrEmpty(request.ModelTopologyJson))
        {
            return BadRequest(new { message = "Model topology JSON is required." });
        }

        if (request.WeightFiles == null || request.WeightFiles.Count == 0)
        {
            return BadRequest(new { message = "At least one weight file is required." });
        }

        // Read weight files
        var weightFiles = new Dictionary<string, byte[]>();
        foreach (var file in request.WeightFiles)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            weightFiles[file.FileName] = memoryStream.ToArray();
        }

        var uploadRequest = new UploadTrainedModelRequest
        {
            Version = request.Version,
            ModelTopologyJson = request.ModelTopologyJson,
            TrainingAccuracy = request.TrainingAccuracy,
            ValidationAccuracy = request.ValidationAccuracy,
            TotalPhotosUsed = request.TotalPhotosUsed,
            TotalProductsUsed = request.TotalProductsUsed,
            TrainingDurationSeconds = request.TrainingDurationSeconds
        };

        var result = await _imageRecognitionService.UploadTrainedModelAsync(
            uploadRequest,
            weightFiles,
            _currentUserService.UserId.Value);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        _logger.LogInformation(
            "Browser-trained model {Version} uploaded by user {UserId}",
            result.Version, _currentUserService.UserId.Value);

        return Ok(result);
    }

    /// <summary>
    /// Gets the training dataset for browser-based model training.
    /// Returns all active products with their photos for client-side training.
    /// </summary>
    /// <returns>Training dataset with photo metadata and URLs.</returns>
    [HttpGet("training-dataset")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(TrainingDatasetResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrainingDatasetResponse>> GetTrainingDataset()
    {
        var dataset = await _imageRecognitionService.GetTrainingDatasetAsync();
        
        if (dataset.TotalPhotos == 0)
        {
            return Ok(new
            {
                dataset.Photos,
                dataset.TotalPhotos,
                dataset.TotalProducts,
                dataset.ClassLabels,
                message = "No product photos available for training. Please upload product photos first."
            });
        }

        return Ok(dataset);
    }
}

/// <summary>
/// Form request for uploading browser-trained model (multipart/form-data).
/// </summary>
public class UploadTrainedModelFormRequest
{
    /// <summary>
    /// Model version identifier (e.g., "v1_20260112").
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Model topology JSON (model.json content).
    /// </summary>
    public string ModelTopologyJson { get; set; } = string.Empty;

    /// <summary>
    /// Training accuracy percentage (0-100).
    /// </summary>
    public double TrainingAccuracy { get; set; }

    /// <summary>
    /// Validation accuracy percentage (0-100).
    /// </summary>
    public double ValidationAccuracy { get; set; }

    /// <summary>
    /// Total photos used for training.
    /// </summary>
    public int TotalPhotosUsed { get; set; }

    /// <summary>
    /// Total products included in training.
    /// </summary>
    public int TotalProductsUsed { get; set; }

    /// <summary>
    /// Training duration in seconds.
    /// </summary>
    public int TrainingDurationSeconds { get; set; }

    /// <summary>
    /// Weight files (.bin files).
    /// </summary>
    public List<IFormFile> WeightFiles { get; set; } = new();
}
