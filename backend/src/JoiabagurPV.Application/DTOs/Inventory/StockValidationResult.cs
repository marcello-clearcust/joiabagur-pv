namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Result of stock validation operation.
/// </summary>
public class StockValidationResult
{
    /// <summary>
    /// Whether the validation succeeded (sufficient stock available).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The available quantity at the point of sale.
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// The requested quantity.
    /// </summary>
    public int RequestedQuantity { get; set; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Warning message (e.g., low stock warning).
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Whether the stock is below minimum threshold (if configured).
    /// </summary>
    public bool IsLowStock { get; set; }
}
