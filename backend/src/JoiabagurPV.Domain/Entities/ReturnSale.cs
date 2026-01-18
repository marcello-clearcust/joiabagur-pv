namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Junction table for the many-to-many relationship between Return and Sale.
/// Tracks which sales were associated with a return and the quantity returned from each.
/// </summary>
public class ReturnSale : BaseEntity
{
    /// <summary>
    /// The return this association belongs to.
    /// </summary>
    public Guid ReturnId { get; set; }

    /// <summary>
    /// The sale being associated with the return.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Quantity returned from this specific sale.
    /// Must be greater than zero and not exceed sale's available-for-return quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price snapshot from the original sale.
    /// Preserves historical accuracy for financial reporting.
    /// </summary>
    public decimal UnitPrice { get; set; }

    // Navigation properties

    /// <summary>
    /// Navigation property for the return.
    /// </summary>
    public virtual Return Return { get; set; } = null!;

    /// <summary>
    /// Navigation property for the sale.
    /// </summary>
    public virtual Sale Sale { get; set; } = null!;

    /// <summary>
    /// Calculates the subtotal for this sale association.
    /// </summary>
    /// <returns>Subtotal (Quantity * UnitPrice).</returns>
    public decimal GetSubtotal() => Quantity * UnitPrice;
}
