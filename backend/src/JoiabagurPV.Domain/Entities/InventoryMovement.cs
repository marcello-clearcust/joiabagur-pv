using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a movement/change in inventory quantity.
/// Provides complete audit trail for stock changes.
/// </summary>
public class InventoryMovement : BaseEntity
{
    /// <summary>
    /// The inventory record this movement is for.
    /// </summary>
    public Guid InventoryId { get; set; }

    /// <summary>
    /// The sale that caused this movement (for Sale type movements).
    /// </summary>
    public Guid? SaleId { get; set; }

    /// <summary>
    /// The return that caused this movement (for Return type movements).
    /// </summary>
    public Guid? ReturnId { get; set; }

    /// <summary>
    /// The user who performed this movement.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The type of movement (Sale, Return, Adjustment, Import).
    /// </summary>
    public MovementType MovementType { get; set; }

    /// <summary>
    /// The change in quantity (positive for additions, negative for reductions).
    /// </summary>
    public int QuantityChange { get; set; }

    /// <summary>
    /// The quantity before this movement.
    /// </summary>
    public int QuantityBefore { get; set; }

    /// <summary>
    /// The quantity after this movement.
    /// </summary>
    public int QuantityAfter { get; set; }

    /// <summary>
    /// The reason for this movement (required for Adjustment type).
    /// Max 500 characters.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When this movement occurred.
    /// </summary>
    public DateTime MovementDate { get; set; }

    // Navigation properties
    public virtual Inventory Inventory { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

