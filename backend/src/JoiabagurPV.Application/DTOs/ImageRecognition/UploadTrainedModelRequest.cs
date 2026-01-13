namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// Request DTO for uploading a browser-trained ML model.
/// </summary>
public class UploadTrainedModelRequest
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
    /// Weight specifications (part of model.json).
    /// </summary>
    public string? WeightSpecs { get; set; }

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
}

/// <summary>
/// Response DTO for model upload result.
/// </summary>
public class UploadTrainedModelResult
{
    /// <summary>
    /// Whether the upload was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if upload failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The uploaded model version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Model metadata after successful upload.
    /// </summary>
    public ModelMetadataDto? Metadata { get; set; }
}
