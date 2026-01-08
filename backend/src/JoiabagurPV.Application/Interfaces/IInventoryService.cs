using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for inventory management operations.
/// </summary>
public interface IInventoryService
{
    #region Assignment Operations

    /// <summary>
    /// Assigns a product to a point of sale.
    /// </summary>
    /// <param name="request">The assignment request.</param>
    /// <returns>The assignment result.</returns>
    Task<AssignmentResult> AssignProductAsync(AssignProductRequest request);

    /// <summary>
    /// Assigns multiple products to a point of sale.
    /// </summary>
    /// <param name="request">The bulk assignment request.</param>
    /// <returns>The bulk assignment result.</returns>
    Task<BulkAssignmentResult> AssignProductsAsync(BulkAssignProductsRequest request);

    /// <summary>
    /// Unassigns a product from a point of sale (soft delete).
    /// </summary>
    /// <param name="request">The unassignment request.</param>
    /// <returns>The assignment result.</returns>
    Task<AssignmentResult> UnassignProductAsync(UnassignProductRequest request);

    /// <summary>
    /// Gets products assigned to a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of assigned inventory records.</returns>
    Task<PaginatedInventoryResult> GetAssignedProductsAsync(Guid pointOfSaleId, int page = 1, int pageSize = 50);

    #endregion

    #region Stock Query Operations

    /// <summary>
    /// Gets stock for a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of inventory records.</returns>
    Task<PaginatedInventoryResult> GetStockByPointOfSaleAsync(Guid pointOfSaleId, int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets centralized stock view (aggregated by product).
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of centralized stock records.</returns>
    Task<PaginatedCentralizedStockResult> GetCentralizedStockAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets stock breakdown for a product across all points of sale.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>Centralized stock with breakdown.</returns>
    Task<CentralizedStockDto?> GetStockBreakdownAsync(Guid productId);

    #endregion

    #region Stock Adjustment Operations

    /// <summary>
    /// Adjusts stock for a product at a point of sale.
    /// </summary>
    /// <param name="request">The adjustment request.</param>
    /// <param name="userId">The ID of the user making the adjustment.</param>
    /// <returns>The adjustment result.</returns>
    Task<StockAdjustmentResult> AdjustStockAsync(StockAdjustmentRequest request, Guid userId);

    #endregion
}

