using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for stock import operations from Excel.
/// </summary>
public interface IStockImportService
{
    /// <summary>
    /// Validates an Excel file for stock import.
    /// </summary>
    /// <param name="stream">The Excel file stream.</param>
    /// <param name="pointOfSaleId">The target point of sale ID.</param>
    /// <returns>Import result with validation errors if any.</returns>
    Task<StockImportResult> ValidateAsync(Stream stream, Guid pointOfSaleId);

    /// <summary>
    /// Imports stock from an Excel file.
    /// </summary>
    /// <param name="stream">The Excel file stream.</param>
    /// <param name="pointOfSaleId">The target point of sale ID.</param>
    /// <param name="userId">The ID of the user performing the import.</param>
    /// <returns>Import result with counts.</returns>
    Task<StockImportResult> ImportAsync(Stream stream, Guid pointOfSaleId, Guid userId);

    /// <summary>
    /// Generates an Excel template for stock import.
    /// </summary>
    /// <returns>A memory stream containing the template Excel file.</returns>
    MemoryStream GenerateTemplate();

    /// <summary>
    /// Gets the allowed file extensions for import.
    /// </summary>
    string[] AllowedExtensions { get; }

    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    long MaxFileSizeBytes { get; }
}

