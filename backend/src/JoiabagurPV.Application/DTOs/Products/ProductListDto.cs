namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// Lightweight DTO for product list display (catalog view).
/// Contains essential fields for catalog browsing without full product details.
/// </summary>
public class ProductListDto
{
    public Guid Id { get; set; }
    public required string SKU { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public string? CollectionName { get; set; }
    public bool IsActive { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

