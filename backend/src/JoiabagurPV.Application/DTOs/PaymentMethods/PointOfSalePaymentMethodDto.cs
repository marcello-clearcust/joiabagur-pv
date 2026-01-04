namespace JoiabagurPV.Application.DTOs.PaymentMethods;

/// <summary>
/// DTO for point of sale payment method assignment information.
/// </summary>
public class PointOfSalePaymentMethodDto
{
    /// <summary>
    /// The assignment's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The payment method ID.
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// The payment method details.
    /// </summary>
    public PaymentMethodDto PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Whether this assignment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When this assignment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this assignment was deactivated (null if still active).
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }
}
