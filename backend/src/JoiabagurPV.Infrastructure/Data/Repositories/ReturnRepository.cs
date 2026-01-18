using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Return entity operations.
/// </summary>
public class ReturnRepository : Repository<Return>, IReturnRepository
{
    private readonly ApplicationDbContext _context;

    public ReturnRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Return?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Returns
            .Include(r => r.Product)
            .Include(r => r.PointOfSale)
            .Include(r => r.User)
            .Include(r => r.Photo)
            .Include(r => r.ReturnSales)
                .ThenInclude(rs => rs.Sale)
                    .ThenInclude(s => s.PaymentMethod)
            .Include(r => r.InventoryMovement)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <inheritdoc/>
    public async Task<(List<Return> Returns, int TotalCount)> GetByPointOfSaleAsync(
        Guid pointOfSaleId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? productId = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.Returns
            .Include(r => r.Product)
            .Include(r => r.User)
            .Include(r => r.Photo)
            .Include(r => r.ReturnSales)
            .Where(r => r.PointOfSaleId == pointOfSaleId);

        query = ApplyFilters(query, startDate, endDate, productId);

        var totalCount = await query.CountAsync();

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (returns, totalCount);
    }

    /// <inheritdoc/>
    public async Task<(List<Return> Returns, int TotalCount)> GetAllReturnsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? pointOfSaleId = null,
        Guid? productId = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.Returns
            .Include(r => r.Product)
            .Include(r => r.PointOfSale)
            .Include(r => r.User)
            .Include(r => r.Photo)
            .Include(r => r.ReturnSales)
            .AsQueryable();

        if (pointOfSaleId.HasValue)
        {
            query = query.Where(r => r.PointOfSaleId == pointOfSaleId.Value);
        }

        query = ApplyFilters(query, startDate, endDate, productId);

        var totalCount = await query.CountAsync();

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (returns, totalCount);
    }

    /// <inheritdoc/>
    public async Task<(List<Return> Returns, int TotalCount)> GetByPointOfSalesAsync(
        IEnumerable<Guid> pointOfSaleIds,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? productId = null,
        int skip = 0,
        int take = 50)
    {
        var posIds = pointOfSaleIds.ToList();

        var query = _context.Returns
            .Include(r => r.Product)
            .Include(r => r.PointOfSale)
            .Include(r => r.User)
            .Include(r => r.Photo)
            .Include(r => r.ReturnSales)
            .Where(r => posIds.Contains(r.PointOfSaleId));

        query = ApplyFilters(query, startDate, endDate, productId);

        var totalCount = await query.CountAsync();

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 50))
            .ToListAsync();

        return (returns, totalCount);
    }

    /// <summary>
    /// Applies common filters to a return query.
    /// </summary>
    private static IQueryable<Return> ApplyFilters(
        IQueryable<Return> query,
        DateTime? startDate,
        DateTime? endDate,
        Guid? productId)
    {
        if (startDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ReturnDate <= endDate.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        return query;
    }
}
