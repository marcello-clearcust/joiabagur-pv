namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a photo associated with a product.
/// </summary>
public class ProductPhoto : BaseEntity
{
    /// <summary>
    /// Foreign key to the product this photo belongs to.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property for the associated product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Name of the stored file.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Display order for sorting photos. Lower values appear first.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether this is the primary/main photo for the product.
    /// Only one photo per product should be marked as primary.
    /// </summary>
    public bool IsPrimary { get; set; } = false;
}




