namespace JoiabagurPV.Application.DTOs.Sales;

/// <summary>
/// Result of creating a sale.
/// </summary>
public class CreateSaleResult
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The created sale DTO.
    /// </summary>
    public SaleDto? Sale { get; set; }

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Warning message (e.g., low stock warning).
    /// </summary>
    public string? WarningMessage { get; set; }

    /// <summary>
    /// Whether stock is low after this sale.
    /// </summary>
    public bool IsLowStock { get; set; }

    /// <summary>
    /// Remaining stock after the sale.
    /// </summary>
    public int RemainingStock { get; set; }
}
