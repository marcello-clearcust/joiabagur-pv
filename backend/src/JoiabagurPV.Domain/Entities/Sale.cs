namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a sale transaction in the system.
/// Provides complete audit trail for sales with automatic inventory updates.
/// </summary>
public class Sale : BaseEntity
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
    /// The user (operator) who registered the sale.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The payment method used for the transaction.
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Price snapshot at the time of sale (frozen from Product.Price).
    /// Preserves price history even if product price changes later.
    /// </summary>
    public decimal Price { get; set; }

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
    /// When the sale occurred.
    /// </summary>
    public DateTime SaleDate { get; set; }

    // Navigation properties
    
    /// <summary>
    /// Navigation property for the sold product.
    /// </summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// Navigation property for the point of sale.
    /// </summary>
    public virtual PointOfSale PointOfSale { get; set; } = null!;

    /// <summary>
    /// Navigation property for the operator who made the sale.
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property for the payment method used.
    /// </summary>
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Navigation property for the optional photo attached to this sale.
    /// </summary>
    public virtual SalePhoto? Photo { get; set; }

    /// <summary>
    /// Navigation property for the corresponding inventory movement.
    /// </summary>
    public virtual InventoryMovement? InventoryMovement { get; set; }

    /// <summary>
    /// Calculates the total amount for this sale.
    /// </summary>
    /// <returns>Total amount (Price * Quantity).</returns>
    public decimal GetTotal() => Price * Quantity;

    /// <summary>
    /// Validates that the quantity is greater than zero.
    /// </summary>
    /// <returns>True if quantity is valid, false otherwise.</returns>
    public bool IsQuantityValid() => Quantity > 0;

    /// <summary>
    /// Validates that the price is greater than zero.
    /// </summary>
    /// <returns>True if price is valid, false otherwise.</returns>
    public bool IsPriceValid() => Price > 0;
}
