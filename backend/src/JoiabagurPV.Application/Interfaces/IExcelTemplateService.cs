namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service for generating Excel templates with consistent formatting.
/// Used by Product Import and Inventory Import services.
/// </summary>
public interface IExcelTemplateService
{
    /// <summary>
    /// Generates an Excel template with the specified configuration.
    /// </summary>
    /// <param name="config">The template configuration.</param>
    /// <returns>A memory stream containing the generated Excel file.</returns>
    MemoryStream GenerateTemplate(ExcelTemplateConfig config);
}

/// <summary>
/// Configuration for generating an Excel template.
/// </summary>
public class ExcelTemplateConfig
{
    /// <summary>
    /// The name of the worksheet.
    /// </summary>
    public string SheetName { get; set; } = "Data";

    /// <summary>
    /// Column definitions for the template.
    /// </summary>
    public List<ExcelColumnConfig> Columns { get; set; } = new();

    /// <summary>
    /// Optional example data rows.
    /// </summary>
    public List<Dictionary<string, object?>> ExampleRows { get; set; } = new();

    /// <summary>
    /// Instructions or comments to add to the first cell.
    /// </summary>
    public string? Instructions { get; set; }
}

/// <summary>
/// Configuration for a single column in the Excel template.
/// </summary>
public class ExcelColumnConfig
{
    /// <summary>
    /// The column header name (must match exactly for import).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether this column is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Data type for validation (Text, Number, Decimal).
    /// </summary>
    public ExcelDataType DataType { get; set; } = ExcelDataType.Text;

    /// <summary>
    /// Column width (optional, auto-fit if not specified).
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// Description/help text for the column.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Minimum value for numeric columns.
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Maximum value for numeric columns.
    /// </summary>
    public decimal? MaxValue { get; set; }
}

/// <summary>
/// Data types for Excel columns.
/// </summary>
public enum ExcelDataType
{
    Text,
    Integer,
    Decimal
}

