using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Sale entity operations.
/// </summary>
public class SaleRepository : Repository<Sale>, ISaleRepository
{
    private readonly ApplicationDbContext _context;

    public SaleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Sale?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Sales
            .Include(s => s.Product)
            .Include(s => s.PointOfSale)
            .Include(s => s.User)
            .Include(s => s.PaymentMethod)
            .Include(s => s.Photo)
            .Include(s => s.InventoryMovement)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<(List<Sale> Sales, int TotalCount)> GetByPointOfSaleAsync(
        Guid pointOfSaleId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? productId = null,
        Guid? userId = null,
        Guid? paymentMethodId = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.Sales
            .Include(s => s.Product)
            .Include(s => s.User)
            .Include(s => s.PaymentMethod)
            .Include(s => s.Photo)
            .Where(s => s.PointOfSaleId == pointOfSaleId);

        query = ApplyFilters(query, startDate, endDate, productId, userId, paymentMethodId);

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (sales, totalCount);
    }

    /// <inheritdoc/>
    public async Task<(List<Sale> Sales, int TotalCount)> GetAllSalesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? pointOfSaleId = null,
        Guid? productId = null,
        Guid? userId = null,
        Guid? paymentMethodId = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.Sales
            .Include(s => s.Product)
            .Include(s => s.PointOfSale)
            .Include(s => s.User)
            .Include(s => s.PaymentMethod)
            .Include(s => s.Photo)
            .AsQueryable();

        if (pointOfSaleId.HasValue)
        {
            query = query.Where(s => s.PointOfSaleId == pointOfSaleId.Value);
        }

        query = ApplyFilters(query, startDate, endDate, productId, userId, paymentMethodId);

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (sales, totalCount);
    }

    /// <inheritdoc/>
    public async Task<(List<Sale> Sales, int TotalCount)> GetByPointOfSalesAsync(
        IEnumerable<Guid> pointOfSaleIds,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? productId = null,
        Guid? userId = null,
        Guid? paymentMethodId = null,
        int skip = 0,
        int take = 50)
    {
        var posIds = pointOfSaleIds.ToList();
        
        var query = _context.Sales
            .Include(s => s.Product)
            .Include(s => s.PointOfSale)
            .Include(s => s.User)
            .Include(s => s.PaymentMethod)
            .Include(s => s.Photo)
            .Where(s => posIds.Contains(s.PointOfSaleId));

        query = ApplyFilters(query, startDate, endDate, productId, userId, paymentMethodId);

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (sales, totalCount);
    }

    /// <summary>
    /// Applies common filters to a sale query.
    /// </summary>
    private static IQueryable<Sale> ApplyFilters(
        IQueryable<Sale> query,
        DateTime? startDate,
        DateTime? endDate,
        Guid? productId,
        Guid? userId,
        Guid? paymentMethodId)
    {
        if (startDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= endDate.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        if (paymentMethodId.HasValue)
        {
            query = query.Where(s => s.PaymentMethodId == paymentMethodId.Value);
        }

        return query;
    }
}
