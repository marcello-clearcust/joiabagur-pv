using JoiabagurPV.Application.DTOs.ImageRecognition;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for image recognition and ML model management operations.
/// </summary>
public interface IImageRecognitionService
{
    /// <summary>
    /// Gets the current active model metadata.
    /// </summary>
    /// <returns>Model metadata if active model exists, null otherwise.</returns>
    Task<ModelMetadataDto?> GetActiveModelMetadataAsync();

    /// <summary>
    /// Gets all model versions (training history).
    /// </summary>
    /// <returns>List of all model versions ordered by training date descending.</returns>
    Task<List<ModelMetadataDto>> GetAllModelVersionsAsync();

    /// <summary>
    /// Gets the model file path for serving to frontend.
    /// </summary>
    /// <param name="version">Optional version. If null, returns active model.</param>
    /// <returns>Model file path if found, null otherwise.</returns>
    Task<string?> GetModelFilePathAsync(string? version = null);

    /// <summary>
    /// Validates that model retraining requirements are met.
    /// </summary>
    /// <returns>Validation result with error message if invalid.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateRetrainingRequirementsAsync();

    /// <summary>
    /// Initiates model retraining as a background job.
    /// </summary>
    /// <param name="initiatedBy">User ID who initiated the training.</param>
    /// <returns>Training job ID.</returns>
    Task<Guid> InitiateModelRetrainingAsync(Guid initiatedBy);

    /// <summary>
    /// Gets the status of a training job.
    /// </summary>
    /// <param name="jobId">Training job ID.</param>
    /// <returns>Job status if found, null otherwise.</returns>
    Task<TrainingJobStatusDto?> GetTrainingJobStatusAsync(Guid jobId);

    /// <summary>
    /// Gets the latest training job.
    /// </summary>
    /// <returns>Latest job status if exists, null otherwise.</returns>
    Task<TrainingJobStatusDto?> GetLatestTrainingJobAsync();

    /// <summary>
    /// Checks if a training job is currently running.
    /// </summary>
    /// <returns>True if training in progress, false otherwise.</returns>
    Task<bool> IsTrainingInProgressAsync();

    /// <summary>
    /// Uploads a browser-trained model.
    /// </summary>
    /// <param name="request">Upload request with model data.</param>
    /// <param name="weightFiles">Weight file data (name -> bytes).</param>
    /// <param name="uploadedBy">User ID who uploaded the model.</param>
    /// <returns>Upload result.</returns>
    Task<UploadTrainedModelResult> UploadTrainedModelAsync(
        UploadTrainedModelRequest request,
        Dictionary<string, byte[]> weightFiles,
        Guid uploadedBy);

    /// <summary>
    /// Gets the training dataset for browser-based training.
    /// Returns all active products with their photos.
    /// </summary>
    /// <returns>Training dataset with photo metadata.</returns>
    Task<TrainingDatasetResponse> GetTrainingDatasetAsync();
}
