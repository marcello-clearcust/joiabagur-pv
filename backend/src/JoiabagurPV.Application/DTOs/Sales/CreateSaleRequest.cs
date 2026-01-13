namespace JoiabagurPV.Application.DTOs.Sales;

/// <summary>
/// Request DTO for creating a sale.
/// </summary>
public class CreateSaleRequest
{
    /// <summary>
    /// The product being sold.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The point of sale where the transaction occurred.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The payment method used for the transaction.
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Quantity of units sold. Must be greater than zero.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional notes or comments about the sale.
    /// Max 500 characters.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional photo data (for image recognition or manual upload).
    /// Base64 encoded string or null if no photo.
    /// </summary>
    public string? PhotoBase64 { get; set; }

    /// <summary>
    /// Original file name for the photo.
    /// </summary>
    public string? PhotoFileName { get; set; }
}
