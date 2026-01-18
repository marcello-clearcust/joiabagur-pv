using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Application.DTOs.Returns;

/// <summary>
/// Request DTO for creating a return.
/// </summary>
public class CreateReturnRequest
{
    /// <summary>
    /// The product being returned.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The point of sale where the return is registered.
    /// Must match the POS where original sale(s) occurred.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// Total quantity being returned.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Category of the return (required).
    /// </summary>
    public ReturnCategory Category { get; set; }

    /// <summary>
    /// Optional free-text reason for the return.
    /// Max 500 characters.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Sale associations with quantities per sale.
    /// Total quantity from all associations must equal Quantity.
    /// </summary>
    public List<SaleAssociationRequest> SaleAssociations { get; set; } = new();

    /// <summary>
    /// Optional photo data (for documenting return condition).
    /// Base64 encoded string or null if no photo.
    /// </summary>
    public string? PhotoBase64 { get; set; }

    /// <summary>
    /// Original file name for the photo.
    /// </summary>
    public string? PhotoFileName { get; set; }
}

/// <summary>
/// Request DTO for associating a sale with a return.
/// </summary>
public class SaleAssociationRequest
{
    /// <summary>
    /// The sale to associate with the return.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Quantity to return from this specific sale.
    /// </summary>
    public int Quantity { get; set; }
}
