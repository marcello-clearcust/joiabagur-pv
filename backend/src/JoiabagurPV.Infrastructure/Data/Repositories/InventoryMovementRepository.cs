using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for InventoryMovement entities.
/// </summary>
public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<List<InventoryMovement>> FindByInventoryAsync(Guid inventoryId, int limit = 50)
    {
        return await _dbSet
            .Include(m => m.User)
            .Where(m => m.InventoryId == inventoryId)
            .OrderByDescending(m => m.MovementDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<(List<InventoryMovement> Movements, int TotalCount)> FindByFiltersAsync(
        Guid? productId = null,
        Guid? pointOfSaleId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _dbSet
            .Include(m => m.User)
            .Include(m => m.Inventory)
                .ThenInclude(i => i.Product)
            .Include(m => m.Inventory)
                .ThenInclude(i => i.PointOfSale)
            .AsQueryable();

        // Apply filters
        if (productId.HasValue)
        {
            query = query.Where(m => m.Inventory.ProductId == productId.Value);
        }

        if (pointOfSaleId.HasValue)
        {
            query = query.Where(m => m.Inventory.PointOfSaleId == pointOfSaleId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(m => m.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(m => m.MovementDate <= endDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var movements = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (movements, totalCount);
    }

    /// <inheritdoc/>
    public IQueryable<MovementSummaryProjection> GetMovementSummaryByProduct(
        DateTime startDate,
        DateTime endDate,
        Guid? pointOfSaleId = null)
    {
        var query = _dbSet
            .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate);

        if (pointOfSaleId.HasValue)
        {
            query = query.Where(m => m.Inventory.PointOfSaleId == pointOfSaleId.Value);
        }

        return query
            .GroupBy(m => new
            {
                m.Inventory.ProductId,
                m.Inventory.Product.Name,
                m.Inventory.Product.SKU
            })
            .Select(g => new MovementSummaryProjection(
                g.Key.ProductId,
                g.Key.Name,
                g.Key.SKU,
                g.Sum(m => m.QuantityChange > 0 ? m.QuantityChange : 0),
                g.Sum(m => m.QuantityChange < 0 ? -m.QuantityChange : 0),
                g.Sum(m => m.QuantityChange > 0 ? m.QuantityChange : 0)
                    - g.Sum(m => m.QuantityChange < 0 ? -m.QuantityChange : 0)
            ));
    }
}

