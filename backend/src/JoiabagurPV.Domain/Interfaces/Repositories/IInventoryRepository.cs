using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Inventory entities.
/// </summary>
public interface IInventoryRepository : IRepository<Inventory>
{
    /// <summary>
    /// Finds an inventory record by product and point of sale.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The inventory record if found, including inactive records.</returns>
    Task<Inventory?> FindByProductAndPointOfSaleAsync(Guid productId, Guid pointOfSaleId);

    /// <summary>
    /// Gets all inventory records for a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="activeOnly">If true, only returns active inventory records.</param>
    /// <returns>List of inventory records.</returns>
    Task<List<Inventory>> FindByPointOfSaleAsync(Guid pointOfSaleId, bool activeOnly = true);

    /// <summary>
    /// Gets all inventory records for a product across all points of sale.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="activeOnly">If true, only returns active inventory records.</param>
    /// <returns>List of inventory records.</returns>
    Task<List<Inventory>> FindByProductAsync(Guid productId, bool activeOnly = true);

    /// <summary>
    /// Gets active inventory records for multiple points of sale.
    /// Used for operator product visibility.
    /// </summary>
    /// <param name="pointOfSaleIds">The point of sale IDs.</param>
    /// <returns>List of active inventory records.</returns>
    Task<List<Inventory>> FindActiveByPointOfSalesAsync(IEnumerable<Guid> pointOfSaleIds);

    /// <summary>
    /// Gets products that are assigned (have inventory) at the given points of sale.
    /// </summary>
    /// <param name="pointOfSaleIds">The point of sale IDs.</param>
    /// <returns>Set of product IDs that have inventory at the given POS.</returns>
    Task<HashSet<Guid>> GetProductIdsWithInventoryAtPointsOfSaleAsync(IEnumerable<Guid> pointOfSaleIds);
}

