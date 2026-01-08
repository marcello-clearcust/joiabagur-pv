namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// Request object for creating a new product.
/// </summary>
public class CreateProductRequest
{
    public required string SKU { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid? CollectionId { get; set; }
}

/// <summary>
/// Request object for updating an existing product.
/// </summary>
public class UpdateProductRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid? CollectionId { get; set; }
    public bool IsActive { get; set; } = true;
}




