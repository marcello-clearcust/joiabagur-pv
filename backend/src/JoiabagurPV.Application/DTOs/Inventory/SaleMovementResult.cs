namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Result of creating a sale movement.
/// </summary>
public class SaleMovementResult
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The created movement record.
    /// </summary>
    public InventoryMovementDto? Movement { get; set; }

    /// <summary>
    /// The updated inventory record.
    /// </summary>
    public InventoryDto? Inventory { get; set; }

    /// <summary>
    /// Quantity before the sale.
    /// </summary>
    public int QuantityBefore { get; set; }

    /// <summary>
    /// Quantity after the sale.
    /// </summary>
    public int QuantityAfter { get; set; }
}
