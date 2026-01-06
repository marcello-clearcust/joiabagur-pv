using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for Excel product import operations.
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Validates an Excel file for product import.
    /// </summary>
    /// <param name="stream">The Excel file stream.</param>
    /// <returns>Import result with validation errors if any.</returns>
    Task<ImportResult> ValidateAsync(Stream stream);

    /// <summary>
    /// Imports products from an Excel file.
    /// </summary>
    /// <param name="stream">The Excel file stream.</param>
    /// <returns>Import result with created/updated counts.</returns>
    Task<ImportResult> ImportAsync(Stream stream);

    /// <summary>
    /// Gets the allowed file extensions for import.
    /// </summary>
    string[] AllowedExtensions { get; }

    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    long MaxFileSizeBytes { get; }
}



