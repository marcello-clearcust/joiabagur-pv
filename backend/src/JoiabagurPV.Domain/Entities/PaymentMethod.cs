namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a payment method available in the system.
/// </summary>
public class PaymentMethod : BaseEntity
{
    /// <summary>
    /// Unique code identifier for the payment method (e.g., CASH, BIZUM, CARD_POS).
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Display name of the payment method.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the payment method.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the payment method is currently active in the system.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for point of sale assignments.
    /// </summary>
    public ICollection<PointOfSalePaymentMethod> PointOfSaleAssignments { get; set; } = new List<PointOfSalePaymentMethod>();
}
