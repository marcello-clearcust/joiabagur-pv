using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Inventory entities.
/// </summary>
public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<Inventory?> FindByProductAndPointOfSaleAsync(Guid productId, Guid pointOfSaleId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.PointOfSale)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.PointOfSaleId == pointOfSaleId);
    }

    /// <inheritdoc/>
    public async Task<List<Inventory>> FindByPointOfSaleAsync(Guid pointOfSaleId, bool activeOnly = true)
    {
        var query = _dbSet
            .Include(i => i.Product)
            .Where(i => i.PointOfSaleId == pointOfSaleId);

        if (activeOnly)
        {
            query = query.Where(i => i.IsActive);
        }

        return await query
            .OrderBy(i => i.Product.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Inventory>> FindByProductAsync(Guid productId, bool activeOnly = true)
    {
        var query = _dbSet
            .Include(i => i.PointOfSale)
            .Where(i => i.ProductId == productId);

        if (activeOnly)
        {
            query = query.Where(i => i.IsActive);
        }

        return await query
            .OrderBy(i => i.PointOfSale.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Inventory>> FindActiveByPointOfSalesAsync(IEnumerable<Guid> pointOfSaleIds)
    {
        var posIds = pointOfSaleIds.ToList();
        
        return await _dbSet
            .Include(i => i.Product)
            .Include(i => i.PointOfSale)
            .Where(i => posIds.Contains(i.PointOfSaleId) && i.IsActive)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<HashSet<Guid>> GetProductIdsWithInventoryAtPointsOfSaleAsync(IEnumerable<Guid> pointOfSaleIds)
    {
        var posIds = pointOfSaleIds.ToList();
        
        var productIds = await _dbSet
            .Where(i => posIds.Contains(i.PointOfSaleId) && i.IsActive)
            .Select(i => i.ProductId)
            .Distinct()
            .ToListAsync();

        return productIds.ToHashSet();
    }
}

