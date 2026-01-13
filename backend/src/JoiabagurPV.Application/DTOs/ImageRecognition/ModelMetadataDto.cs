namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// DTO for model metadata.
/// </summary>
public class ModelMetadataDto
{
    /// <summary>
    /// Model version identifier.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// When this model was trained.
    /// </summary>
    public DateTime TrainedAt { get; set; }

    /// <summary>
    /// Model file path.
    /// </summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Accuracy metrics (JSON).
    /// </summary>
    public string? AccuracyMetrics { get; set; }

    /// <summary>
    /// Total photos used for training.
    /// </summary>
    public int TotalPhotosUsed { get; set; }

    /// <summary>
    /// Total products included in training.
    /// </summary>
    public int TotalProductsUsed { get; set; }

    /// <summary>
    /// Whether this is the active model.
    /// </summary>
    public bool IsActive { get; set; }
}
