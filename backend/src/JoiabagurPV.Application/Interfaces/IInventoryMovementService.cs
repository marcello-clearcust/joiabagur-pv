using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for inventory movement history operations.
/// </summary>
public interface IInventoryMovementService
{
    /// <summary>
    /// Gets movement history with optional filters.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>Paginated list of movements.</returns>
    Task<PaginatedMovementResult> GetMovementHistoryAsync(MovementHistoryFilter filter);

    /// <summary>
    /// Gets movements for a specific inventory record.
    /// </summary>
    /// <param name="inventoryId">The inventory ID.</param>
    /// <param name="limit">Maximum number of movements to return.</param>
    /// <returns>List of movements.</returns>
    Task<List<InventoryMovementDto>> GetMovementsByInventoryAsync(Guid inventoryId, int limit = 50);
}

