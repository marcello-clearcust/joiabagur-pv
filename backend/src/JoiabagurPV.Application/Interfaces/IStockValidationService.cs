using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for stock validation operations.
/// Used primarily by sales-management to validate stock availability before sale.
/// </summary>
public interface IStockValidationService
{
    /// <summary>
    /// Validates that sufficient stock is available for a sale.
    /// </summary>
    /// <param name="productId">The ID of the product to validate.</param>
    /// <param name="pointOfSaleId">The ID of the point of sale.</param>
    /// <param name="requestedQuantity">The quantity requested for sale.</param>
    /// <returns>Validation result with availability information and warnings.</returns>
    Task<StockValidationResult> ValidateStockAvailabilityAsync(
        Guid productId, 
        Guid pointOfSaleId, 
        int requestedQuantity);
}
