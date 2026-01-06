namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// Result of a product import operation.
/// </summary>
public class ImportResult
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
    /// Number of products created.
    /// </summary>
    public int CreatedCount { get; set; }

    /// <summary>
    /// Number of products updated.
    /// </summary>
    public int UpdatedCount { get; set; }

    /// <summary>
    /// Number of collections created automatically.
    /// </summary>
    public int CollectionsCreatedCount { get; set; }

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

/// <summary>
/// Represents a single import error.
/// </summary>
public class ImportError
{
    /// <summary>
    /// The row number in the Excel file (1-based, including header).
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// The field/column with the error.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The value that caused the error.
    /// </summary>
    public string? Value { get; set; }
}



