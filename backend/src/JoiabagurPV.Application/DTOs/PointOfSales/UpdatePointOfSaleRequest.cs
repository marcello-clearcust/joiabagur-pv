namespace JoiabagurPV.Application.DTOs.PointOfSales;

/// <summary>
/// Request DTO for updating an existing point of sale.
/// </summary>
public class UpdatePointOfSaleRequest
{
    /// <summary>
    /// The display name for the point of sale.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The physical address (optional).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The contact phone number (optional).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// The contact email (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the point of sale is active.
    /// </summary>
    public bool IsActive { get; set; }
}
