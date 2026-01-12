using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for stock validation operations.
/// Validates stock availability for sales and provides low stock warnings.
/// </summary>
public class StockValidationService : IStockValidationService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<StockValidationService> _logger;

    // Low stock threshold percentage (10% of current quantity)
    private const decimal LowStockThresholdPercentage = 0.10m;
    private const int MinimumLowStockThreshold = 5;

    public StockValidationService(
        IInventoryRepository inventoryRepository,
        ILogger<StockValidationService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<StockValidationResult> ValidateStockAvailabilityAsync(
        Guid productId,
        Guid pointOfSaleId,
        int requestedQuantity)
    {
        // Validate input
        if (requestedQuantity <= 0)
        {
            return new StockValidationResult
            {
                IsValid = false,
                AvailableQuantity = 0,
                RequestedQuantity = requestedQuantity,
                ErrorMessage = "La cantidad solicitada debe ser mayor que cero."
            };
        }

        // Check if product is assigned to the point of sale
        var inventory = await _inventoryRepository.FindByProductAndPointOfSaleAsync(productId, pointOfSaleId);

        if (inventory == null || !inventory.IsActive)
        {
            _logger.LogWarning(
                "Stock validation failed: Product {ProductId} not assigned to POS {PointOfSaleId}",
                productId, pointOfSaleId);

            return new StockValidationResult
            {
                IsValid = false,
                AvailableQuantity = 0,
                RequestedQuantity = requestedQuantity,
                ErrorMessage = "El producto no está asignado a este punto de venta."
            };
        }

        // Check if sufficient quantity is available
        var availableQuantity = inventory.Quantity;

        if (availableQuantity < requestedQuantity)
        {
            _logger.LogWarning(
                "Stock validation failed: Insufficient stock for product {ProductId} at POS {PointOfSaleId}. Available: {Available}, Requested: {Requested}",
                productId, pointOfSaleId, availableQuantity, requestedQuantity);

            return new StockValidationResult
            {
                IsValid = false,
                AvailableQuantity = availableQuantity,
                RequestedQuantity = requestedQuantity,
                ErrorMessage = $"Stock insuficiente. Disponible: {availableQuantity}, Solicitado: {requestedQuantity}."
            };
        }

        // Check for low stock warning
        var remainingAfterSale = availableQuantity - requestedQuantity;
        var lowStockThreshold = Math.Max(
            (int)(availableQuantity * LowStockThresholdPercentage),
            MinimumLowStockThreshold);

        var isLowStock = remainingAfterSale <= lowStockThreshold && remainingAfterSale > 0;

        var result = new StockValidationResult
        {
            IsValid = true,
            AvailableQuantity = availableQuantity,
            RequestedQuantity = requestedQuantity,
            IsLowStock = isLowStock
        };

        if (isLowStock)
        {
            result.WarningMessage = $"Advertencia: Stock bajo. Después de esta venta quedarán {remainingAfterSale} unidades.";
            
            _logger.LogInformation(
                "Low stock warning for product {ProductId} at POS {PointOfSaleId}. Remaining after sale: {Remaining}",
                productId, pointOfSaleId, remainingAfterSale);
        }

        _logger.LogDebug(
            "Stock validation passed for product {ProductId} at POS {PointOfSaleId}. Available: {Available}, Requested: {Requested}",
            productId, pointOfSaleId, availableQuantity, requestedQuantity);

        return result;
    }
}
