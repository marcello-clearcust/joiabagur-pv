using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for inventory management operations.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryService> _logger;

    private const int MaxPageSize = 50;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IInventoryMovementRepository movementRepository,
        IProductRepository productRepository,
        IPointOfSaleRepository pointOfSaleRepository,
        IUnitOfWork unitOfWork,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _pointOfSaleRepository = pointOfSaleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Assignment Operations

    /// <inheritdoc/>
    public async Task<AssignmentResult> AssignProductAsync(AssignProductRequest request)
    {
        // Validate product exists and is active
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = $"Producto con ID '{request.ProductId}' no encontrado."
            };
        }

        if (!product.IsActive)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = $"No se puede asignar un producto inactivo."
            };
        }

        // Validate point of sale exists and is active
        var pointOfSale = await _pointOfSaleRepository.GetByIdAsync(request.PointOfSaleId);
        if (pointOfSale == null)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = $"Punto de venta con ID '{request.PointOfSaleId}' no encontrado."
            };
        }

        if (!pointOfSale.IsActive)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = $"No se puede asignar a un punto de venta inactivo."
            };
        }

        // Check if assignment already exists
        var existingInventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(
            request.ProductId, request.PointOfSaleId);

        if (existingInventory != null)
        {
            if (existingInventory.IsActive)
            {
                return new AssignmentResult
                {
                    Success = false,
                    ErrorMessage = "El producto ya está asignado a este punto de venta."
                };
            }

            // Reactivate previously unassigned product (preserve quantity per design decision)
            existingInventory.IsActive = true;
            existingInventory.LastUpdatedAt = DateTime.UtcNow;

            await _inventoryRepository.UpdateAsync(existingInventory);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Product {ProductId} reactivated at POS {PointOfSaleId} with quantity {Quantity}",
                request.ProductId, request.PointOfSaleId, existingInventory.Quantity);

            return new AssignmentResult
            {
                Success = true,
                Inventory = MapToDto(existingInventory, product, pointOfSale),
                WasReactivated = true
            };
        }

        // Create new inventory assignment
        var inventory = new Inventory
        {
            ProductId = request.ProductId,
            PointOfSaleId = request.PointOfSaleId,
            Quantity = 0,
            IsActive = true,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _inventoryRepository.AddAsync(inventory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} assigned to POS {PointOfSaleId}",
            request.ProductId, request.PointOfSaleId);

        return new AssignmentResult
        {
            Success = true,
            Inventory = MapToDto(inventory, product, pointOfSale)
        };
    }

    /// <inheritdoc/>
    public async Task<BulkAssignmentResult> AssignProductsAsync(BulkAssignProductsRequest request)
    {
        var result = new BulkAssignmentResult();

        // Validate point of sale exists and is active
        var pointOfSale = await _pointOfSaleRepository.GetByIdAsync(request.PointOfSaleId);
        if (pointOfSale == null)
        {
            return new BulkAssignmentResult
            {
                Success = false,
                Message = $"Punto de venta con ID '{request.PointOfSaleId}' no encontrado."
            };
        }

        if (!pointOfSale.IsActive)
        {
            return new BulkAssignmentResult
            {
                Success = false,
                Message = $"No se puede asignar a un punto de venta inactivo."
            };
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var productId in request.ProductIds)
            {
                var assignResult = await AssignProductAsync(new AssignProductRequest
                {
                    ProductId = productId,
                    PointOfSaleId = request.PointOfSaleId
                });

                if (assignResult.Success)
                {
                    if (assignResult.WasReactivated)
                    {
                        result.ReactivatedCount++;
                    }
                    else
                    {
                        result.AssignedCount++;
                    }

                    if (assignResult.Inventory != null)
                    {
                        result.Inventories.Add(assignResult.Inventory);
                    }
                }
                else if (assignResult.ErrorMessage?.Contains("ya está asignado") == true)
                {
                    result.SkippedCount++;
                }
                else
                {
                    result.FailedCount++;
                    result.Errors.Add(new AssignmentError
                    {
                        ProductId = productId,
                        Message = assignResult.ErrorMessage ?? "Error desconocido"
                    });
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            result.Success = result.FailedCount == 0;
            result.Message = $"Asignación completada: {result.AssignedCount} asignados, " +
                           $"{result.ReactivatedCount} reactivados, {result.SkippedCount} omitidos, " +
                           $"{result.FailedCount} fallidos.";

            _logger.LogInformation(
                "Bulk assignment to POS {PointOfSaleId}: {Assigned} assigned, {Reactivated} reactivated, {Skipped} skipped, {Failed} failed",
                request.PointOfSaleId, result.AssignedCount, result.ReactivatedCount, result.SkippedCount, result.FailedCount);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error during bulk assignment, transaction rolled back");
            throw;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<AssignmentResult> UnassignProductAsync(UnassignProductRequest request)
    {
        var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(
            request.ProductId, request.PointOfSaleId);

        if (inventory == null || !inventory.IsActive)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = "El producto no está asignado a este punto de venta."
            };
        }

        // Validate quantity is 0 before unassignment
        if (inventory.Quantity > 0)
        {
            return new AssignmentResult
            {
                Success = false,
                ErrorMessage = $"No se puede desasignar un producto con stock ({inventory.Quantity} unidades). " +
                             "Primero ajuste el stock a 0."
            };
        }

        // Soft delete
        inventory.IsActive = false;
        inventory.LastUpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} unassigned from POS {PointOfSaleId}",
            request.ProductId, request.PointOfSaleId);

        return new AssignmentResult
        {
            Success = true,
            Inventory = MapToDto(inventory, inventory.Product, inventory.PointOfSale)
        };
    }

    /// <inheritdoc/>
    public async Task<PaginatedInventoryResult> GetAssignedProductsAsync(Guid pointOfSaleId, int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), MaxPageSize);

        var inventories = await _inventoryRepository.FindByPointOfSaleAsync(pointOfSaleId, activeOnly: true);

        var totalCount = inventories.Count;
        var pagedItems = inventories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => MapToDto(i, i.Product, i.PointOfSale))
            .ToList();

        return new PaginatedInventoryResult
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    #endregion

    #region Stock Query Operations

    /// <inheritdoc/>
    public async Task<PaginatedInventoryResult> GetStockByPointOfSaleAsync(Guid pointOfSaleId, int page = 1, int pageSize = 50)
    {
        // Same as GetAssignedProductsAsync for now
        return await GetAssignedProductsAsync(pointOfSaleId, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<PaginatedCentralizedStockResult> GetCentralizedStockAsync(int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(Math.Max(1, pageSize), MaxPageSize);

        // Get all active products with their inventory
        var products = await _productRepository.GetAllAsync(includeInactive: false);
        var productIds = products.Select(p => p.Id).ToList();
        
        // Get all inventory for these products
        var allInventories = new List<Inventory>();
        foreach (var productId in productIds)
        {
            var productInventories = await _inventoryRepository.FindByProductAsync(productId, activeOnly: true);
            allInventories.AddRange(productInventories);
        }

        // Group by product
        var groupedByProduct = allInventories
            .GroupBy(i => i.ProductId)
            .Select(g => new CentralizedStockDto
            {
                ProductId = g.Key,
                ProductSku = g.First().Product?.SKU ?? string.Empty,
                ProductName = g.First().Product?.Name ?? string.Empty,
                TotalQuantity = g.Sum(i => i.Quantity),
                Breakdown = g.Select(i => new PointOfSaleStockDto
                {
                    PointOfSaleId = i.PointOfSaleId,
                    PointOfSaleName = i.PointOfSale?.Name ?? string.Empty,
                    Quantity = i.Quantity
                }).ToList()
            })
            .OrderBy(s => s.ProductName)
            .ToList();

        var totalCount = groupedByProduct.Count;
        var pagedItems = groupedByProduct
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedCentralizedStockResult
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<CentralizedStockDto?> GetStockBreakdownAsync(Guid productId)
    {
        var inventories = await _inventoryRepository.FindByProductAsync(productId, activeOnly: true);

        if (!inventories.Any())
        {
            return null;
        }

        var firstInventory = inventories.First();
        return new CentralizedStockDto
        {
            ProductId = productId,
            ProductSku = firstInventory.Product?.SKU ?? string.Empty,
            ProductName = firstInventory.Product?.Name ?? string.Empty,
            TotalQuantity = inventories.Sum(i => i.Quantity),
            Breakdown = inventories.Select(i => new PointOfSaleStockDto
            {
                PointOfSaleId = i.PointOfSaleId,
                PointOfSaleName = i.PointOfSale?.Name ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    #endregion

    #region Stock Adjustment Operations

    /// <inheritdoc/>
    public async Task<StockAdjustmentResult> AdjustStockAsync(StockAdjustmentRequest request, Guid userId)
    {
        // Validate reason is provided
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return new StockAdjustmentResult
            {
                Success = false,
                ErrorMessage = "El motivo del ajuste es requerido."
            };
        }

        if (request.Reason.Length > 500)
        {
            return new StockAdjustmentResult
            {
                Success = false,
                ErrorMessage = "El motivo del ajuste no puede exceder 500 caracteres."
            };
        }

        // Find inventory
        var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(
            request.ProductId, request.PointOfSaleId);

        if (inventory == null || !inventory.IsActive)
        {
            return new StockAdjustmentResult
            {
                Success = false,
                ErrorMessage = "El producto no está asignado a este punto de venta."
            };
        }

        // Calculate new quantity
        var quantityBefore = inventory.Quantity;
        var quantityAfter = quantityBefore + request.QuantityChange;

        // Validate non-negative stock
        if (quantityAfter < 0)
        {
            return new StockAdjustmentResult
            {
                Success = false,
                ErrorMessage = $"El ajuste resultaría en stock negativo ({quantityAfter}). Stock actual: {quantityBefore}."
            };
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Update inventory
            inventory.Quantity = quantityAfter;
            inventory.LastUpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(inventory);

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                UserId = userId,
                MovementType = MovementType.Adjustment,
                QuantityChange = request.QuantityChange,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                Reason = request.Reason,
                MovementDate = DateTime.UtcNow
            };

            await _movementRepository.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Stock adjusted for product {ProductId} at POS {PointOfSaleId}: {Before} -> {After} ({Change})",
                request.ProductId, request.PointOfSaleId, quantityBefore, quantityAfter, request.QuantityChange);

            return new StockAdjustmentResult
            {
                Success = true,
                Inventory = MapToDto(inventory, inventory.Product, inventory.PointOfSale),
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error during stock adjustment, transaction rolled back");
            throw;
        }
    }

    #endregion

    #region Sales Integration Operations

    /// <inheritdoc/>
    public async Task<SaleMovementResult> CreateSaleMovementAsync(
        Guid productId,
        Guid pointOfSaleId,
        Guid saleId,
        int quantity,
        Guid userId)
    {
        // Validate quantity
        if (quantity <= 0)
        {
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "La cantidad debe ser mayor que cero."
            };
        }

        // Find inventory
        var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(productId, pointOfSaleId);

        if (inventory == null || !inventory.IsActive)
        {
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "El producto no está asignado a este punto de venta."
            };
        }

        // Validate sufficient stock
        if (inventory.Quantity < quantity)
        {
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = $"Stock insuficiente. Disponible: {inventory.Quantity}, Solicitado: {quantity}."
            };
        }

        var quantityBefore = inventory.Quantity;
        var quantityAfter = quantityBefore - quantity;

        // NOTE: This method is designed to be called within an existing transaction
        // (e.g., from SalesService), so it does NOT manage its own transaction.
        // The caller is responsible for transaction management and commit/rollback.

        try
        {
            // Update inventory (atomic operation)
            inventory.Quantity = quantityAfter;
            inventory.LastUpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(inventory);

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                SaleId = saleId,
                UserId = userId,
                MovementType = MovementType.Sale,
                QuantityChange = -quantity, // Negative for sales
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                Reason = null, // Sales don't need a reason
                MovementDate = DateTime.UtcNow
            };

            await _movementRepository.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Sale movement created for product {ProductId} at POS {PointOfSaleId} (Sale {SaleId}): {Before} -> {After} (-{Quantity})",
                productId, pointOfSaleId, saleId, quantityBefore, quantityAfter, quantity);

            return new SaleMovementResult
            {
                Success = true,
                Inventory = MapToDto(inventory, inventory.Product, inventory.PointOfSale),
                Movement = MapMovementToDto(movement, inventory),
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sale movement creation");
            
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "Error al crear el movimiento de venta."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SaleMovementResult> CreateReturnMovementAsync(
        Guid productId,
        Guid pointOfSaleId,
        Guid returnId,
        int quantity,
        Guid userId)
    {
        // Validate quantity
        if (quantity <= 0)
        {
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "La cantidad debe ser mayor que cero."
            };
        }

        // Find inventory
        var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(productId, pointOfSaleId);

        if (inventory == null || !inventory.IsActive)
        {
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "El producto no está asignado a este punto de venta."
            };
        }

        var quantityBefore = inventory.Quantity;
        var quantityAfter = quantityBefore + quantity; // Positive for returns (adding stock back)

        // NOTE: This method is designed to be called within an existing transaction
        // (e.g., from ReturnService), so it does NOT manage its own transaction.
        // The caller is responsible for transaction management and commit/rollback.

        try
        {
            // Update inventory (atomic operation)
            inventory.Quantity = quantityAfter;
            inventory.LastUpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(inventory);

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryId = inventory.Id,
                ReturnId = returnId,
                UserId = userId,
                MovementType = MovementType.Return,
                QuantityChange = quantity, // Positive for returns
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                Reason = null, // Returns don't need a reason in movement (reason is in Return entity)
                MovementDate = DateTime.UtcNow
            };

            await _movementRepository.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Return movement created for product {ProductId} at POS {PointOfSaleId} (Return {ReturnId}): {Before} -> {After} (+{Quantity})",
                productId, pointOfSaleId, returnId, quantityBefore, quantityAfter, quantity);

            return new SaleMovementResult
            {
                Success = true,
                Inventory = MapToDto(inventory, inventory.Product, inventory.PointOfSale),
                Movement = MapMovementToDto(movement, inventory),
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during return movement creation");
            
            return new SaleMovementResult
            {
                Success = false,
                ErrorMessage = "Error al crear el movimiento de devolución."
            };
        }
    }

    #endregion

    #region Private Methods

    private static InventoryDto MapToDto(Inventory inventory, Product? product, PointOfSale? pointOfSale)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductSku = product?.SKU ?? string.Empty,
            ProductName = product?.Name ?? string.Empty,
            PointOfSaleId = inventory.PointOfSaleId,
            PointOfSaleName = pointOfSale?.Name ?? string.Empty,
            Quantity = inventory.Quantity,
            IsActive = inventory.IsActive,
            LastUpdatedAt = inventory.LastUpdatedAt,
            CreatedAt = inventory.CreatedAt
        };
    }

    private static InventoryMovementDto MapMovementToDto(InventoryMovement movement, Inventory inventory)
    {
        return new InventoryMovementDto
        {
            Id = movement.Id,
            InventoryId = movement.InventoryId,
            ProductId = inventory.ProductId,
            ProductSku = inventory.Product?.SKU ?? string.Empty,
            ProductName = inventory.Product?.Name ?? string.Empty,
            PointOfSaleId = inventory.PointOfSaleId,
            PointOfSaleName = inventory.PointOfSale?.Name ?? string.Empty,
            MovementType = movement.MovementType,
            QuantityChange = movement.QuantityChange,
            QuantityBefore = movement.QuantityBefore,
            QuantityAfter = movement.QuantityAfter,
            Reason = movement.Reason,
            UserId = movement.UserId,
            UserName = movement.User?.FullName ?? string.Empty,
            MovementDate = movement.MovementDate,
            CreatedAt = movement.CreatedAt
        };
    }

    #endregion
}

