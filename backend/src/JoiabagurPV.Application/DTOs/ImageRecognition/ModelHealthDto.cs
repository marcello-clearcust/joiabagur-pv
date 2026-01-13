namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// DTO for model health metrics and alert level.
/// </summary>
public class ModelHealthDto
{
    /// <summary>
    /// Current model version (null if no model exists).
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// When the current model was trained (null if no model exists).
    /// </summary>
    public DateTime? LastTrainedAt { get; set; }

    /// <summary>
    /// Days since last training.
    /// </summary>
    public int? DaysSinceTraining { get; set; }

    /// <summary>
    /// Alert level (CRITICAL, HIGH, RECOMMENDED, OK).
    /// </summary>
    public string AlertLevel { get; set; } = "OK";

    /// <summary>
    /// Alert message/recommendation.
    /// </summary>
    public string? AlertMessage { get; set; }

    /// <summary>
    /// Catalog metrics.
    /// </summary>
    public CatalogMetricsDto CatalogMetrics { get; set; } = new();

    /// <summary>
    /// Photo metrics.
    /// </summary>
    public PhotoMetricsDto PhotoMetrics { get; set; } = new();

    /// <summary>
    /// Precision metrics (optional, Phase 2).
    /// </summary>
    public PrecisionMetricsDto? PrecisionMetrics { get; set; }
}

/// <summary>
/// Catalog metrics for model health.
/// </summary>
public class CatalogMetricsDto
{
    /// <summary>
    /// Total products in catalog.
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Products with at least one photo.
    /// </summary>
    public int ProductsWithPhotos { get; set; }

    /// <summary>
    /// Products without photos.
    /// </summary>
    public int ProductsWithoutPhotos { get; set; }

    /// <summary>
    /// Products added since last training.
    /// </summary>
    public int NewProductsSinceTraining { get; set; }

    /// <summary>
    /// Percentage of new products.
    /// </summary>
    public double NewProductsPercentage { get; set; }
}

/// <summary>
/// Photo metrics for model health.
/// </summary>
public class PhotoMetricsDto
{
    /// <summary>
    /// Total photos in system.
    /// </summary>
    public int TotalPhotos { get; set; }

    /// <summary>
    /// Photos added since last training.
    /// </summary>
    public int PhotosAddedSinceTraining { get; set; }

    /// <summary>
    /// Photos deleted since last training.
    /// </summary>
    public int PhotosDeletedSinceTraining { get; set; }

    /// <summary>
    /// Net change percentage.
    /// </summary>
    public double NetChangePercentage { get; set; }
}

/// <summary>
/// Precision metrics for model health (Phase 2 - optional in MVP).
/// </summary>
public class PrecisionMetricsDto
{
    /// <summary>
    /// Top-1 accuracy (percentage).
    /// </summary>
    public double? Top1Accuracy { get; set; }

    /// <summary>
    /// Top-3 accuracy (percentage).
    /// </summary>
    public double? Top3Accuracy { get; set; }

    /// <summary>
    /// Average inference time in milliseconds.
    /// </summary>
    public double? AverageInferenceTimeMs { get; set; }

    /// <summary>
    /// Fallback to manual entry rate (percentage).
    /// </summary>
    public double? FallbackRate { get; set; }
}
