namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Request to adjust stock for a product at a point of sale.
/// </summary>
public class StockAdjustmentRequest
{
    /// <summary>
    /// The product ID to adjust.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The point of sale ID where the adjustment is made.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The quantity change (positive for increase, negative for decrease).
    /// </summary>
    public int QuantityChange { get; set; }

    /// <summary>
    /// The reason for the adjustment (required, max 500 characters).
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of a stock adjustment operation.
/// </summary>
public class StockAdjustmentResult
{
    /// <summary>
    /// Whether the adjustment was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The updated inventory record.
    /// </summary>
    public InventoryDto? Inventory { get; set; }

    /// <summary>
    /// The quantity before the adjustment.
    /// </summary>
    public int QuantityBefore { get; set; }

    /// <summary>
    /// The quantity after the adjustment.
    /// </summary>
    public int QuantityAfter { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

