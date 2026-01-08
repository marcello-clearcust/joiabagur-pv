using ClosedXML.Excel;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for stock import operations from Excel.
/// </summary>
public class StockImportService : IStockImportService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelTemplateService _templateService;
    private readonly ILogger<StockImportService> _logger;

    // Required columns (exact names)
    private const string ColSku = "SKU";
    private const string ColQuantity = "Quantity";

    public StockImportService(
        IInventoryRepository inventoryRepository,
        IInventoryMovementRepository movementRepository,
        IProductRepository productRepository,
        IPointOfSaleRepository pointOfSaleRepository,
        IUnitOfWork unitOfWork,
        IExcelTemplateService templateService,
        ILogger<StockImportService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _pointOfSaleRepository = pointOfSaleRepository;
        _unitOfWork = unitOfWork;
        _templateService = templateService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string[] AllowedExtensions => new[] { ".xlsx", ".xls" };

    /// <inheritdoc/>
    public long MaxFileSizeBytes => 10 * 1024 * 1024; // 10 MB

    /// <inheritdoc/>
    public MemoryStream GenerateTemplate()
    {
        var config = new ExcelTemplateConfig
        {
            SheetName = "Stock Import",
            Instructions = "Fill in stock data starting from row 2. Both columns are required. " +
                          "The SKU must exist in the product catalog. " +
                          "Quantity values will be ADDED to existing stock. " +
                          "Products not assigned to the point of sale will be automatically assigned. " +
                          "Example rows are shown in gray italic - delete or overwrite them with your data.",
            Columns = new List<ExcelColumnConfig>
            {
                new()
                {
                    Name = ColSku,
                    IsRequired = true,
                    DataType = ExcelDataType.Text,
                    Width = 20,
                    Description = "Product SKU (required). Must exist in the product catalog."
                },
                new()
                {
                    Name = ColQuantity,
                    IsRequired = true,
                    DataType = ExcelDataType.Integer,
                    Width = 12,
                    MinValue = 0,
                    Description = "Quantity to add (required). Must be 0 or greater."
                }
            },
            ExampleRows = new List<Dictionary<string, object?>>
            {
                new()
                {
                    { ColSku, "RING-001" },
                    { ColQuantity, 10 }
                },
                new()
                {
                    { ColSku, "NECK-002" },
                    { ColQuantity, 25 }
                }
            }
        };

        return _templateService.GenerateTemplate(config);
    }

    /// <inheritdoc/>
    public async Task<StockImportResult> ValidateAsync(Stream stream, Guid pointOfSaleId)
    {
        var result = new StockImportResult();

        try
        {
            // Validate point of sale exists
            var pointOfSale = await _pointOfSaleRepository.GetByIdAsync(pointOfSaleId);
            if (pointOfSale == null)
            {
                result.Success = false;
                result.Message = "Punto de venta no encontrado.";
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Field = "PointOfSaleId",
                    Message = $"Punto de venta con ID '{pointOfSaleId}' no encontrado."
                });
                return result;
            }

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
                result.Message = "Formato de archivo inválido: faltan columnas requeridas.";
                return result;
            }

            // Parse and validate rows
            var rowsData = ParseRows(worksheet, columnMap);
            result.TotalRows = rowsData.Count;

            // Validate each row
            var validationErrors = await ValidateRowsAsync(rowsData);
            result.Errors.AddRange(validationErrors);

            result.Success = !result.Errors.Any();
            result.Message = result.Success 
                ? $"Validación exitosa para {result.TotalRows} filas." 
                : $"Se encontraron {result.Errors.Count} error(es) de validación.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock import file");
            result.Success = false;
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Field = "File",
                Message = "Formato de archivo Excel inválido."
            });
            result.Message = "Error al leer el archivo Excel.";
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<StockImportResult> ImportAsync(Stream stream, Guid pointOfSaleId, Guid userId)
    {
        var result = new StockImportResult();

        try
        {
            // Reset stream position if possible
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            // Validate point of sale exists and is active
            var pointOfSale = await _pointOfSaleRepository.GetByIdAsync(pointOfSaleId);
            if (pointOfSale == null)
            {
                result.Success = false;
                result.Message = "Punto de venta no encontrado.";
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Field = "PointOfSaleId",
                    Message = $"Punto de venta con ID '{pointOfSaleId}' no encontrado."
                });
                return result;
            }

            if (!pointOfSale.IsActive)
            {
                result.Success = false;
                result.Message = "No se puede importar a un punto de venta inactivo.";
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Field = "PointOfSaleId",
                    Message = "El punto de venta está inactivo."
                });
                return result;
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
                result.Message = "Formato de archivo inválido: faltan columnas requeridas.";
                return result;
            }

            // Parse rows
            var rowsData = ParseRows(worksheet, columnMap);
            result.TotalRows = rowsData.Count;

            // Validate rows
            var validationErrors = await ValidateRowsAsync(rowsData);
            if (validationErrors.Any())
            {
                result.Errors.AddRange(validationErrors);
                result.Success = false;
                result.Message = $"Se encontraron {validationErrors.Count} error(es) de validación. Importación cancelada.";
                return result;
            }

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Get products by SKU
                var skus = rowsData.Select(r => r.Sku.ToUpperInvariant()).ToList();
                var products = await _productRepository.GetBySkusAsync(skus);

                foreach (var row in rowsData)
                {
                    var normalizedSku = row.Sku.ToUpperInvariant();
                    if (!products.TryGetValue(normalizedSku, out var product))
                    {
                        continue; // Already validated, shouldn't happen
                    }

                    // Check if inventory exists
                    var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(
                        product.Id, pointOfSaleId);

                    if (inventory == null)
                    {
                        // Create new inventory (implicit assignment)
                        inventory = new Inventory
                        {
                            ProductId = product.Id,
                            PointOfSaleId = pointOfSaleId,
                            Quantity = 0,
                            IsActive = true,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepository.AddAsync(inventory);
                        await _unitOfWork.SaveChangesAsync();
                        
                        result.AssignmentsCreatedCount++;
                        result.Warnings.Add($"Producto '{row.Sku}' asignado automáticamente al punto de venta.");
                    }
                    else if (!inventory.IsActive)
                    {
                        // Reactivate if previously unassigned
                        inventory.IsActive = true;
                        inventory.LastUpdatedAt = DateTime.UtcNow;
                        await _inventoryRepository.UpdateAsync(inventory);
                        
                        result.AssignmentsCreatedCount++;
                        result.Warnings.Add($"Producto '{row.Sku}' reactivado en el punto de venta.");
                    }

                    // Add quantity and create movement
                    if (row.Quantity > 0)
                    {
                        var quantityBefore = inventory.Quantity;
                        inventory.Quantity += row.Quantity;
                        inventory.LastUpdatedAt = DateTime.UtcNow;
                        await _inventoryRepository.UpdateAsync(inventory);

                        // Create movement record
                        var movement = new InventoryMovement
                        {
                            InventoryId = inventory.Id,
                            UserId = userId,
                            MovementType = MovementType.Import,
                            QuantityChange = row.Quantity,
                            QuantityBefore = quantityBefore,
                            QuantityAfter = inventory.Quantity,
                            Reason = "Importación desde Excel",
                            MovementDate = DateTime.UtcNow
                        };
                        await _movementRepository.AddAsync(movement);

                        result.StockUpdatedCount++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.Message = $"Importación exitosa: {result.StockUpdatedCount} productos con stock actualizado, " +
                               $"{result.AssignmentsCreatedCount} asignaciones creadas.";

                _logger.LogInformation(
                    "Stock import completed for POS {PointOfSaleId}: {Updated} updated, {Assigned} assigned",
                    pointOfSaleId, result.StockUpdatedCount, result.AssignmentsCreatedCount);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error during stock import, transaction rolled back");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing stock from Excel");
            result.Success = false;
            result.Errors.Add(new ImportError
            {
                RowNumber = 0,
                Field = "File",
                Message = $"Error de importación: {ex.Message}"
            });
            result.Message = "La importación falló debido a un error.";
        }

        return result;
    }

    #region Private Methods

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
        var requiredColumns = new[] { ColSku, ColQuantity };

        foreach (var col in requiredColumns)
        {
            if (!columnMap.ContainsKey(col))
            {
                errors.Add(new ImportError
                {
                    RowNumber = 1,
                    Field = col,
                    Message = $"La columna requerida '{col}' no existe."
                });
            }
        }

        return errors;
    }

    private static List<StockRowData> ParseRows(IXLWorksheet worksheet, Dictionary<string, int> columnMap)
    {
        var rows = new List<StockRowData>();
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

            var quantityStr = GetCellValue(row, columnMap, ColQuantity);
            int.TryParse(quantityStr, out var quantity);

            rows.Add(new StockRowData
            {
                RowNumber = rowNum,
                Sku = sku.Trim(),
                Quantity = quantity,
                QuantityRaw = quantityStr
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

    private async Task<List<ImportError>> ValidateRowsAsync(List<StockRowData> rows)
    {
        var errors = new List<ImportError>();
        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Get all SKUs at once for validation
        var skus = rows.Select(r => r.Sku.ToUpperInvariant()).ToList();
        var existingProducts = await _productRepository.GetBySkusAsync(skus);

        foreach (var row in rows)
        {
            var normalizedSku = row.Sku.ToUpperInvariant();

            // Check for duplicate SKUs within file
            if (seenSkus.Contains(normalizedSku))
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
                seenSkus.Add(normalizedSku);
            }

            // Validate SKU exists in catalog
            if (!existingProducts.ContainsKey(normalizedSku))
            {
                errors.Add(new ImportError
                {
                    RowNumber = row.RowNumber,
                    Field = ColSku,
                    Message = $"Producto con SKU '{row.Sku}' no encontrado en el catálogo.",
                    Value = row.Sku
                });
            }
            else
            {
                var product = existingProducts[normalizedSku];
                if (!product.IsActive)
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = row.RowNumber,
                        Field = ColSku,
                        Message = $"Producto con SKU '{row.Sku}' está inactivo.",
                        Value = row.Sku
                    });
                }
            }

            // Validate Quantity >= 0
            if (row.Quantity < 0)
            {
                errors.Add(new ImportError
                {
                    RowNumber = row.RowNumber,
                    Field = ColQuantity,
                    Message = "La cantidad debe ser 0 o mayor.",
                    Value = row.QuantityRaw
                });
            }
        }

        return errors;
    }

    private class StockRowData
    {
        public int RowNumber { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityRaw { get; set; } = string.Empty;
    }

    #endregion
}

