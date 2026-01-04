namespace JoiabagurPV.Application.DTOs.PointOfSales;

/// <summary>
/// DTO for point of sale information.
/// </summary>
public class PointOfSaleDto
{
    /// <summary>
    /// The point of sale's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The point of sale's display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The point of sale's unique code.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// The physical address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The contact email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the point of sale is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the point of sale was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the point of sale was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
