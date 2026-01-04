namespace JoiabagurPV.Application.DTOs.PaymentMethods;

/// <summary>
/// DTO for payment method information.
/// </summary>
public class PaymentMethodDto
{
    /// <summary>
    /// The payment method's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The payment method's unique code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The payment method's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the payment method is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the payment method was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the payment method was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
