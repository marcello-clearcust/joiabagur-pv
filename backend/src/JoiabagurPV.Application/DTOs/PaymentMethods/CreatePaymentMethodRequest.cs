namespace JoiabagurPV.Application.DTOs.PaymentMethods;

/// <summary>
/// Request DTO for creating a new payment method.
/// </summary>
public class CreatePaymentMethodRequest
{
    /// <summary>
    /// Unique code for the payment method.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Display name for the payment method.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }
}
