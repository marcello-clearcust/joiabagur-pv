namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// Data transfer object for product information.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public required string SKU { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductPhotoDto> Photos { get; set; } = new();
}

/// <summary>
/// Data transfer object for collection information.
/// </summary>
public class CollectionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ProductCount { get; set; }
}




