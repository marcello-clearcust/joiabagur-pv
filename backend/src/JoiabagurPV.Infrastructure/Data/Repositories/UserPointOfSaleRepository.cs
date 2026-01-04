using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for UserPointOfSale entity.
/// </summary>
public class UserPointOfSaleRepository : Repository<UserPointOfSale>, IUserPointOfSaleRepository
{
    private readonly ApplicationDbContext _context;

    public UserPointOfSaleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<UserPointOfSale>> GetByUserIdAsync(Guid userId, bool activeOnly = false)
    {
        var query = _context.UserPointOfSales.Where(ups => ups.UserId == userId);

        if (activeOnly)
        {
            query = query.Where(ups => ups.IsActive);
        }

        return await query.OrderByDescending(ups => ups.AssignedAt).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<UserPointOfSale>> GetByPointOfSaleAsync(Guid pointOfSaleId, bool activeOnly = false)
    {
        var query = _context.UserPointOfSales
            .Include(ups => ups.User)
            .Where(ups => ups.PointOfSaleId == pointOfSaleId);

        if (activeOnly)
        {
            query = query.Where(ups => ups.IsActive);
        }

        return await query.OrderBy(ups => ups.User.LastName).ThenBy(ups => ups.User.FirstName).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<UserPointOfSale?> GetAssignmentAsync(Guid userId, Guid pointOfSaleId)
    {
        return await _context.UserPointOfSales
            .FirstOrDefaultAsync(ups => ups.UserId == userId && ups.PointOfSaleId == pointOfSaleId);
    }

    /// <inheritdoc/>
    public async Task<UserPointOfSale?> GetActiveAssignmentAsync(Guid userId, Guid pointOfSaleId)
    {
        return await _context.UserPointOfSales
            .FirstOrDefaultAsync(ups => 
                ups.UserId == userId && 
                ups.PointOfSaleId == pointOfSaleId && 
                ups.IsActive);
    }

    /// <inheritdoc/>
    public async Task<bool> HasAccessAsync(Guid userId, Guid pointOfSaleId)
    {
        return await _context.UserPointOfSales
            .AnyAsync(ups => 
                ups.UserId == userId && 
                ups.PointOfSaleId == pointOfSaleId && 
                ups.IsActive);
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> GetAssignedPointOfSaleIdsAsync(Guid userId)
    {
        return await _context.UserPointOfSales
            .Where(ups => ups.UserId == userId && ups.IsActive)
            .Select(ups => ups.PointOfSaleId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CountActiveAssignmentsAsync(Guid userId)
    {
        return await _context.UserPointOfSales
            .CountAsync(ups => ups.UserId == userId && ups.IsActive);
    }
}
