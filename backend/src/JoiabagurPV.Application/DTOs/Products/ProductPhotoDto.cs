namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// DTO for product photo information.
/// </summary>
public class ProductPhotoDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string FileName { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}



