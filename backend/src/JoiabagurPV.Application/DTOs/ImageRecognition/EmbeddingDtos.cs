namespace JoiabagurPV.Application.DTOs.ImageRecognition;

/// <summary>
/// Request to save a single photo embedding.
/// </summary>
public class SaveEmbeddingRequest
{
    /// <summary>
    /// ID of the product photo.
    /// </summary>
    public Guid PhotoId { get; set; }

    /// <summary>
    /// ID of the product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// 1280-dimensional MobileNetV2 feature vector.
    /// </summary>
    public required float[] Vector { get; set; }
}

/// <summary>
/// Single embedding entry returned in the embeddings index.
/// </summary>
public class EmbeddingDto
{
    /// <summary>
    /// ID of the product photo.
    /// </summary>
    public Guid PhotoId { get; set; }

    /// <summary>
    /// ID of the product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// 1280-dimensional MobileNetV2 feature vector.
    /// </summary>
    public required float[] Vector { get; set; }
}

/// <summary>
/// Response containing all stored embeddings for client-side similarity search.
/// </summary>
public class EmbeddingsIndexResponse
{
    /// <summary>
    /// All stored embeddings.
    /// </summary>
    public List<EmbeddingDto> Embeddings { get; set; } = new();

    /// <summary>
    /// Timestamp of the most recently updated embedding.
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Total count of stored embeddings.
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Lightweight response for checking whether the embeddings index is stale.
/// </summary>
public class EmbeddingsStatusResponse
{
    /// <summary>
    /// Total count of stored embeddings.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Timestamp of the most recently updated embedding.
    /// </summary>
    public DateTime? LastUpdated { get; set; }
}
