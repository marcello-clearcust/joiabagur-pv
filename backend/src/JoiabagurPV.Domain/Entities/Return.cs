using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a product return in the system.
/// Provides complete audit trail for returns with automatic inventory updates.
/// </summary>
public class Return : BaseEntity
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
    /// The user (operator) who registered the return.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Total quantity being returned.
    /// Must be less than or equal to sum of associated sales' available quantities.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Category of the return (required).
    /// Used for analytics and pattern detection.
    /// </summary>
    public ReturnCategory Category { get; set; }

    /// <summary>
    /// Optional free-text reason for the return.
    /// Max 500 characters.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When the return occurred.
    /// </summary>
    public DateTime ReturnDate { get; set; }

    // Navigation properties

    /// <summary>
    /// Navigation property for the returned product.
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// Navigation property for the point of sale.
    /// </summary>
    public virtual PointOfSale PointOfSale { get; set; } = null!;

    /// <summary>
    /// Navigation property for the operator who registered the return.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property for associated sales (many-to-many through ReturnSale).
    /// </summary>
    public virtual ICollection<ReturnSale> ReturnSales { get; set; } = new List<ReturnSale>();

    /// <summary>
    /// Navigation property for the optional photo attached to this return.
    /// </summary>
    public virtual ReturnPhoto? Photo { get; set; }

    /// <summary>
    /// Navigation property for the corresponding inventory movement.
    /// </summary>
    public virtual InventoryMovement? InventoryMovement { get; set; }

    /// <summary>
    /// Calculates the total return value based on associated sales' prices.
    /// </summary>
    /// <returns>Total return value.</returns>
    public decimal GetTotalValue() => ReturnSales.Sum(rs => rs.Quantity * rs.UnitPrice);

    /// <summary>
    /// Validates that the quantity is greater than zero.
    /// </summary>
    /// <returns>True if quantity is valid, false otherwise.</returns>
    public bool IsQuantityValid() => Quantity > 0;
}
