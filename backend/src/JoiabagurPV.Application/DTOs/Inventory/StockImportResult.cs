using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Result of a stock import operation.
/// </summary>
public class StockImportResult
{
    /// <summary>
    /// Whether the import was successful overall.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total number of rows processed.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of products that had stock added.
    /// </summary>
    public int StockUpdatedCount { get; set; }

    /// <summary>
    /// Number of products that were implicitly assigned.
    /// </summary>
    public int AssignmentsCreatedCount { get; set; }

    /// <summary>
    /// List of validation errors preventing import.
    /// </summary>
    public List<ImportError> Errors { get; set; } = new();

    /// <summary>
    /// List of warnings (non-blocking issues).
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Summary message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

