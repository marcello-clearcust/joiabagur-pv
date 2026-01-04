namespace JoiabagurPV.Application.DTOs.PointOfSales;

/// <summary>
/// Request DTO for creating a new point of sale.
/// </summary>
public class CreatePointOfSaleRequest
{
    /// <summary>
    /// The display name for the point of sale.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The unique code identifier.
    /// </summary>
    public required string Code { get; set; }

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
}
