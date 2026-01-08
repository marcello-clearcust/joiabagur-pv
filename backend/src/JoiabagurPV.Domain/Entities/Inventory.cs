namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents the inventory of a product at a specific point of sale.
/// Tracks stock quantity and assignment status.
/// </summary>
public class Inventory : BaseEntity
{
    /// <summary>
    /// The ID of the product this inventory record is for.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The ID of the point of sale this inventory is assigned to.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The current quantity of the product at this point of sale.
    /// Must be non-negative.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Whether this inventory assignment is active.
    /// Soft delete uses IsActive = false.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the inventory was last updated (quantity or status change).
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual PointOfSale PointOfSale { get; set; } = null!;
    public virtual ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
}

