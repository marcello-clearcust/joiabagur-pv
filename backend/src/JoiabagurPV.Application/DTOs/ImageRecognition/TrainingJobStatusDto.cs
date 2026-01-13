namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// DTO for training job status.
/// </summary>
public class TrainingJobStatusDto
{
    /// <summary>
    /// Job ID.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Current status (Queued, InProgress, Completed, Failed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Current stage description.
    /// </summary>
    public string? CurrentStage { get; set; }

    /// <summary>
    /// When the job started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the job completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Resulting model version if successful.
    /// </summary>
    public string? ResultModelVersion { get; set; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    public int? DurationSeconds { get; set; }
}
