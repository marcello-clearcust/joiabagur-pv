namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Stores a MobileNetV2 feature embedding vector for a product photo.
/// Used for cosine similarity-based image recognition inference.
/// </summary>
public class ProductPhotoEmbedding : BaseEntity
{
    /// <summary>
    /// Foreign key to the product photo this embedding belongs to.
    /// </summary>
    public Guid ProductPhotoId { get; set; }

    /// <summary>
    /// Navigation property for the associated product photo.
    /// </summary>
    public ProductPhoto? ProductPhoto { get; set; }

    /// <summary>
    /// Foreign key to the product this embedding belongs to.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property for the associated product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Product SKU for fast lookups without joining products table.
    /// </summary>
    public required string ProductSku { get; set; }

    /// <summary>
    /// 1280-dimensional MobileNetV2 feature vector stored as JSON text.
    /// </summary>
    public required string EmbeddingVector { get; set; }
}
