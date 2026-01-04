namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents the assignment of a payment method to a point of sale.
/// </summary>
public class PointOfSalePaymentMethod : BaseEntity
{
    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The payment method ID.
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Whether this payment method is currently active for this point of sale.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this assignment was deactivated (null if still active).
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }

    /// <summary>
    /// Navigation property for the point of sale.
    /// </summary>
    public PointOfSale PointOfSale { get; set; } = null!;

    /// <summary>
    /// Navigation property for the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
