using ClosedXML.Excel;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for Excel product import operations.
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly IProductRepository _productRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExcelImportService> _logger;

    // Required columns
    private const string ColSku = "SKU";
    private const string ColName = "Name";
    private const string ColDescription = "Description";
    private const string ColPrice = "Price";
    private const string ColCollection = "Collection";

    public ExcelImportService(
        IProductRepository productRepository,
        ICollectionRepository collectionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExcelImportService> logger)
    {
        _productRepository = productRepository;
        _collectionRepository = collectionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string[] AllowedExtensions => new[] { ".xlsx", ".xls" };

    /// <inheritdoc/>
    public long MaxFileSizeBytes => 10 * 1024 * 1024; // 10 MB

    /// <inheritdoc/>
    public async Task<ImportResult> ValidateAsync(Stream stream)
    {
        var result = new ImportResult();

        try
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Validate headers
            var headerRow = worksheet.Row(1);
            var columnMap = GetColumnMap(headerRow);

            var headerErrors = ValidateHeaders(columnMap);
            if (headerErrors.Any())
            {
                result.Errors.AddRange(headerErrors);
                result.Success = false;
                result.Message = "Invalid file format: missing required columns.";
                return result;
            }

            // Parse and validate rows
            var rowsData = ParseRows(worksheet, columnMap);
            result.TotalRows = rowsData.Count;

            // Validate each row
            var validationErrors = ValidateRows(rowsData);
            result.Errors.AddRange(validationErrors);

            result.Success = !result.Errors.Any();
            result.Message = result.Success 
                ? $"Validation passed for {result.TotalRows} rows." 
                : $"Found {result.Errors.Count} validation error(s).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Excel file");
            result.Success = false;
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Field = "File",
                Message = "Invalid Excel file format."
            });
            result.Message = "Failed to read Excel file.";
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ImportResult> ImportAsync(Stream stream)
    {
        var result = new ImportResult();

        try
        {
            // Reset stream position if possible
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            // Get column mapping
            var headerRow = worksheet.Row(1);
            var columnMap = GetColumnMap(headerRow);

            var headerErrors = ValidateHeaders(columnMap);
            if (headerErrors.Any())
            {
                result.Errors.AddRange(headerErrors);
                result.Success = false;
                result.Message = "Invalid file format: missing required columns.";
                return result;
            }

            // Parse rows
            var rowsData = ParseRows(worksheet, columnMap);
            result.TotalRows = rowsData.Count;

            // Validate rows
            var validationErrors = ValidateRows(rowsData);
            if (validationErrors.Any())
            {
                result.Errors.AddRange(validationErrors);
                result.Success = false;
                result.Message = $"Found {validationErrors.Count} validation error(s). Import cancelled.";
                return result;
            }

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Get existing products by SKU
                var skus = rowsData.Select(r => r.Sku).ToList();
                var existingProducts = await _productRepository.GetBySkusAsync(skus);

                // Get/create collections
                var collectionNames = rowsData
                    .Where(r => !string.IsNullOrWhiteSpace(r.CollectionName))
                    .Select(r => r.CollectionName!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingCollections = await _collectionRepository.GetByNamesAsync(collectionNames);
                var collectionsToCreate = new List<Collection>();

                foreach (var collectionName in collectionNames)
                {
                    if (!existingCollections.ContainsKey(collectionName.ToLower()))
                    {
                        var newCollection = new Collection { Name = collectionName };
                        collectionsToCreate.Add(newCollection);
                        existingCollections[collectionName.ToLower()] = newCollection;
                    }
                }

                if (collectionsToCreate.Any())
                {
                    await _collectionRepository.AddRangeAsync(collectionsToCreate);
                    await _unitOfWork.SaveChangesAsync();
                    result.CollectionsCreatedCount = collectionsToCreate.Count;
                    _logger.LogInformation("Created {Count} new collections", collectionsToCreate.Count);
                }

                // Process products
                var productsToCreate = new List<Product>();
                var productsToUpdate = new List<Product>();

                foreach (var row in rowsData)
                {
                    Guid? collectionId = null;
                    if (!string.IsNullOrWhiteSpace(row.CollectionName))
                    {
                        var collection = existingCollections[row.CollectionName.ToLower()];
                        collectionId = collection.Id;
                    }

                    if (existingProducts.TryGetValue(row.Sku, out var existingProduct))
                    {
                        // Update existing product
                        existingProduct.Name = row.Name;
                        existingProduct.Description = row.Description;
                        existingProduct.Price = row.Price;
                        existingProduct.CollectionId = collectionId;
                        productsToUpdate.Add(existingProduct);
                    }
                    else
                    {
                        // Create new product
                        var newProduct = new Product
                        {
                            SKU = row.Sku,
                            Name = row.Name,
                            Description = row.Description,
                            Price = row.Price,
                            CollectionId = collectionId,
                            IsActive = true
                        };
                        productsToCreate.Add(newProduct);
                    }
                }

                if (productsToCreate.Any())
                {
                    await _productRepository.AddRangeAsync(productsToCreate);
                    result.CreatedCount = productsToCreate.Count;
                }

                if (productsToUpdate.Any())
                {
                    await _productRepository.UpdateRangeAsync(productsToUpdate);
                    result.UpdatedCount = productsToUpdate.Count;
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.Message = $"Import successful: {result.CreatedCount} created, {result.UpdatedCount} updated.";

                _logger.LogInformation(
                    "Product import completed: {Created} created, {Updated} updated, {Collections} collections created",
                    result.CreatedCount, result.UpdatedCount, result.CollectionsCreatedCount);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error during product import, transaction rolled back");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from Excel");
            result.Success = false;
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Field = "File",
                Message = $"Import failed: {ex.Message}"
            });
            result.Message = "Import failed due to an error.";
        }

        return result;
    }

    private static Dictionary<string, int> GetColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastColumn = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

        for (int col = 1; col <= lastColumn; col++)
        {
            var cellValue = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrEmpty(cellValue))
            {
                map[cellValue] = col;
            }
        }

        return map;
    }

    private static List<ImportError> ValidateHeaders(Dictionary<string, int> columnMap)
    {
        var errors = new List<ImportError>();
        var requiredColumns = new[] { ColSku, ColName, ColPrice };

        foreach (var col in requiredColumns)
        {
            if (!columnMap.ContainsKey(col))
            {
                errors.Add(new ImportError
                {
                    RowNumber = 1,
                    Field = col,
                    Message = $"Required column '{col}' is missing."
                });
            }
        }

        return errors;
    }

    private static List<ProductRowData> ParseRows(IXLWorksheet worksheet, Dictionary<string, int> columnMap)
    {
        var rows = new List<ProductRowData>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int rowNum = 2; rowNum <= lastRow; rowNum++)
        {
            var row = worksheet.Row(rowNum);

            // Skip empty rows
            if (row.IsEmpty())
            {
                continue;
            }

            var sku = GetCellValue(row, columnMap, ColSku);
            
            // Skip rows with empty SKU
            if (string.IsNullOrWhiteSpace(sku))
            {
                continue;
            }

            var priceStr = GetCellValue(row, columnMap, ColPrice);
            decimal.TryParse(priceStr, out var price);

            rows.Add(new ProductRowData
            {
                RowNumber = rowNum,
                Sku = sku,
                Name = GetCellValue(row, columnMap, ColName),
                Description = GetCellValue(row, columnMap, ColDescription),
                Price = price,
                PriceRaw = priceStr,
                CollectionName = GetCellValue(row, columnMap, ColCollection)
            });
        }

        return rows;
    }

    private static string GetCellValue(IXLRow row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var colIndex))
        {
            return string.Empty;
        }

        return row.Cell(colIndex).GetString().Trim();
    }

    private static List<ImportError> ValidateRows(List<ProductRowData> rows)
    {
        var errors = new List<ImportError>();
        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            // Validate SKU not empty
            if (string.IsNullOrWhiteSpace(row.Sku))
            {
                errors.Add(new ImportError
                {
                    RowNumber = row.RowNumber,
                    Field = ColSku,
                    Message = "SKU is required.",
                    Value = row.Sku
                });
            }
            else
            {
                // Check for duplicate SKUs within file
                if (seenSkus.Contains(row.Sku))
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = row.RowNumber,
                        Field = ColSku,
                        Message = $"SKU duplicado en el archivo: {row.Sku}",
                        Value = row.Sku
                    });
                }
                else
                {
                    seenSkus.Add(row.Sku);
                }
            }

            // Validate Name not empty
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                errors.Add(new ImportError
                {
                    RowNumber = row.RowNumber,
                    Field = ColName,
                    Message = "Name is required.",
                    Value = row.Name
                });
            }

            // Validate Price > 0
            if (row.Price <= 0)
            {
                errors.Add(new ImportError
                {
                    RowNumber = row.RowNumber,
                    Field = ColPrice,
                    Message = "Price must be greater than zero.",
                    Value = row.PriceRaw
                });
            }
        }

        return errors;
    }

    private class ProductRowData
    {
        public int RowNumber { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string PriceRaw { get; set; } = string.Empty;
        public string? CollectionName { get; set; }
    }
}



