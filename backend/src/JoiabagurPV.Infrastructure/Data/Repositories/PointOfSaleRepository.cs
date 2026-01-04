using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PointOfSale entity operations.
/// </summary>
public class PointOfSaleRepository : Repository<PointOfSale>, IPointOfSaleRepository
{
    private readonly ApplicationDbContext _context;

    public PointOfSaleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PointOfSale?> GetByCodeAsync(string code)
    {
        return await _context.PointOfSales
            .FirstOrDefaultAsync(pos => pos.Code == code.ToUpperInvariant());
    }

    /// <inheritdoc/>
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var upperCode = code.ToUpperInvariant();
        
        if (excludeId.HasValue)
        {
            return await _context.PointOfSales
                .AnyAsync(pos => pos.Code == upperCode && pos.Id != excludeId.Value);
        }

        return await _context.PointOfSales
            .AnyAsync(pos => pos.Code == upperCode);
    }

    /// <inheritdoc/>
    public async Task<List<PointOfSale>> GetAllAsync(bool includeInactive = true)
    {
        Console.WriteLine($"PointOfSaleRepository.GetAllAsync: includeInactive={includeInactive}");

        var query = _context.PointOfSales.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(pos => pos.IsActive);
        }

        var result = await query
            .OrderBy(pos => pos.Name)
            .ToListAsync();

        Console.WriteLine($"PointOfSaleRepository.GetAllAsync: Returning {result.Count} POS");
        foreach (var pos in result)
        {
            Console.WriteLine($"  POS: {pos.Name} (Code: {pos.Code}, IsActive: {pos.IsActive})");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<PointOfSale>> GetByUserAsync(Guid userId, bool includeInactive = false)
    {
        var query = _context.UserPointOfSales
            .Where(ups => ups.UserId == userId);

        if (!includeInactive)
        {
            query = query.Where(ups => ups.IsActive);
        }

        return await query
            .Join(_context.PointOfSales.Where(pos => pos.IsActive),
                ups => ups.PointOfSaleId,
                pos => pos.Id,
                (ups, pos) => pos)
            .OrderBy(pos => pos.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PointOfSale?> GetWithAssignmentsAsync(Guid pointOfSaleId)
    {
        return await _context.PointOfSales
            .Include(pos => pos.OperatorAssignments)
            .FirstOrDefaultAsync(pos => pos.Id == pointOfSaleId);
    }

    /// <inheritdoc/>
    public async Task<bool> HasActiveAssignmentsAsync(Guid pointOfSaleId)
    {
        return await _context.UserPointOfSales
            .AnyAsync(ups => ups.PointOfSaleId == pointOfSaleId && ups.IsActive);
    }
}
