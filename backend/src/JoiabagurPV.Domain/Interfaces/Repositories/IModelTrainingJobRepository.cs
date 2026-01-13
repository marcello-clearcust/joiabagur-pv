using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ModelTrainingJob entity operations.
/// </summary>
public interface IModelTrainingJobRepository : IRepository<ModelTrainingJob>
{
    /// <summary>
    /// Gets the latest training job.
    /// </summary>
    /// <returns>The most recent training job if exists, null otherwise.</returns>
    Task<ModelTrainingJob?> GetLatestJobAsync();

    /// <summary>
    /// Gets all training jobs ordered by creation date descending.
    /// </summary>
    /// <param name="limit">Maximum number of jobs to return.</param>
    /// <returns>List of training jobs.</returns>
    Task<List<ModelTrainingJob>> GetRecentJobsAsync(int limit = 10);

    /// <summary>
    /// Checks if a training job is currently in progress.
    /// </summary>
    /// <returns>True if a job is running, false otherwise.</returns>
    Task<bool> IsJobInProgressAsync();

    /// <summary>
    /// Gets the currently running job if exists.
    /// </summary>
    /// <returns>The in-progress job if exists, null otherwise.</returns>
    Task<ModelTrainingJob?> GetInProgressJobAsync();
}
