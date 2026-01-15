namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// DTO for a single photo in the training dataset.
/// </summary>
public class TrainingPhotoDto
{
    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name (used as class label).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Photo ID.
    /// </summary>
    public Guid PhotoId { get; set; }

    /// <summary>
    /// Photo URL for downloading.
    /// </summary>
    public string PhotoUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for the full training dataset response.
/// </summary>
public class TrainingDatasetResponse
{
    /// <summary>
    /// List of all photos for training.
    /// </summary>
    public List<TrainingPhotoDto> Photos { get; set; } = new();

    /// <summary>
    /// Total number of photos.
    /// </summary>
    public int TotalPhotos { get; set; }

    /// <summary>
    /// Total number of products with photos.
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Product class labels (unique product names).
    /// </summary>
    public List<string> ClassLabels { get; set; } = new();
}

/// <summary>
/// DTO for mapping a class label to product details (used for inference).
/// </summary>
public class ProductLabelMapping
{
    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name (class label).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Sample photo URL for display.
    /// </summary>
    public string PhotoUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO for class labels response (used by inference/operators).
/// Contains only the data needed to map model predictions to products.
/// </summary>
public class ClassLabelsResponse
{
    /// <summary>
    /// List of class labels (product names) in order matching model output.
    /// </summary>
    public List<string> ClassLabels { get; set; } = new();

    /// <summary>
    /// Mapping from class label (product name) to product details.
    /// </summary>
    public Dictionary<string, ProductLabelMapping> ProductMappings { get; set; } = new();
}
