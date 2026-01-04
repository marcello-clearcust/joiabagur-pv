namespace JoiabagurPV.Application.DTOs.PaymentMethods;

/// <summary>
/// Request DTO for updating a payment method.
/// </summary>
public class UpdatePaymentMethodRequest
{
    /// <summary>
    /// Display name for the payment method.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }
}
