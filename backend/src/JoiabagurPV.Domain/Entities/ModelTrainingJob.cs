namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a model training job.
/// Tracks the status and progress of ML model training operations.
/// </summary>
public class ModelTrainingJob : BaseEntity
{
    /// <summary>
    /// The user who initiated the training job.
    /// </summary>
    public Guid InitiatedBy { get; set; }

    /// <summary>
    /// Current status of the training job.
    /// </summary>
    public required string Status { get; set; } // Queued, InProgress, Completed, Failed

    /// <summary>
    /// Current progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage { get; set; } = 0;

    /// <summary>
    /// Current training stage description (e.g., "Downloading photos", "Training epoch 5/20").
    /// </summary>
    public string? CurrentStage { get; set; }

    /// <summary>
    /// When the training started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the training completed (successfully or with failure).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if training failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The model version that was created (if successful).
    /// </summary>
    public string? ResultModelVersion { get; set; }

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Navigation property for the user who initiated training.
    /// </summary>
    public virtual User InitiatedByUser { get; set; } = null!;
}
