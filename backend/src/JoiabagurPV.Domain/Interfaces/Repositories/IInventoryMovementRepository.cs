using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for InventoryMovement entities.
/// </summary>
public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    /// <summary>
    /// Gets movements for a specific inventory record.
    /// </summary>
    /// <param name="inventoryId">The inventory ID.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <returns>List of movements ordered by date descending.</returns>
    Task<List<InventoryMovement>> FindByInventoryAsync(Guid inventoryId, int limit = 50);

    /// <summary>
    /// Gets movements with optional filters.
    /// </summary>
    /// <param name="productId">Optional product ID filter.</param>
    /// <param name="pointOfSaleId">Optional point of sale ID filter.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of movements ordered by date descending.</returns>
    Task<(List<InventoryMovement> Movements, int TotalCount)> FindByFiltersAsync(
        Guid? productId = null,
        Guid? pointOfSaleId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50);
}

