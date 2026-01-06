namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a product collection/category for organizing products.
/// </summary>
public class Collection : BaseEntity
{
    /// <summary>
    /// Display name of the collection.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the collection.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property for products in this collection.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}



