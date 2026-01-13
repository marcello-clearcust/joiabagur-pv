namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents metadata for an AI model version.
/// Tracks training history, accuracy metrics, and model file locations.
/// </summary>
public class ModelMetadata : BaseEntity
{
    /// <summary>
    /// Model version identifier (e.g., "v2_20260111").
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// When this model was trained.
    /// </summary>
    public DateTime TrainedAt { get; set; }

    /// <summary>
    /// Path to the model files in storage (directory containing .json and .bin files).
    /// </summary>
    public required string ModelPath { get; set; }

    /// <summary>
    /// JSON-serialized accuracy metrics (e.g., {"top1": 0.65, "top3": 0.78}).
    /// </summary>
    public string? AccuracyMetrics { get; set; }

    /// <summary>
    /// Total number of photos used for training.
    /// </summary>
    public int TotalPhotosUsed { get; set; }

    /// <summary>
    /// Total number of products included in training.
    /// </summary>
    public int TotalProductsUsed { get; set; }

    /// <summary>
    /// Whether this model is currently active (latest deployed version).
    /// Only one model can be active at a time.
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Additional notes about this model version.
    /// </summary>
    public string? Notes { get; set; }
}
