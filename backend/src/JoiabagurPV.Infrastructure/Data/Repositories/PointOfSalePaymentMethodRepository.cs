using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PointOfSalePaymentMethod entity operations.
/// </summary>
public class PointOfSalePaymentMethodRepository : Repository<PointOfSalePaymentMethod>, IPointOfSalePaymentMethodRepository
{
    private readonly ApplicationDbContext _context;

    public PointOfSalePaymentMethodRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<PointOfSalePaymentMethod>> GetByPointOfSaleAsync(Guid pointOfSaleId, bool includeInactive = false)
    {
        var baseQuery = _context.PointOfSalePaymentMethods
            .Where(pospm => pospm.PointOfSaleId == pointOfSaleId)
            .Include(pospm => pospm.PaymentMethod)
            .AsQueryable();

        if (!includeInactive)
        {
            baseQuery = baseQuery.Where(pospm => pospm.IsActive && pospm.PaymentMethod.IsActive);
        }

        return await baseQuery
            .OrderBy(pospm => pospm.PaymentMethod.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PointOfSalePaymentMethod?> GetAssignmentAsync(Guid pointOfSaleId, Guid paymentMethodId)
    {
        return await _context.PointOfSalePaymentMethods
            .Include(pospm => pospm.PaymentMethod)
            .FirstOrDefaultAsync(pospm => 
                pospm.PointOfSaleId == pointOfSaleId && 
                pospm.PaymentMethodId == paymentMethodId);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAssignedAsync(Guid pointOfSaleId, Guid paymentMethodId)
    {
        return await _context.PointOfSalePaymentMethods
            .AnyAsync(pospm => 
                pospm.PointOfSaleId == pointOfSaleId && 
                pospm.PaymentMethodId == paymentMethodId);
    }

    /// <inheritdoc/>
    public async Task<bool> IsActiveForPointOfSaleAsync(Guid pointOfSaleId, Guid paymentMethodId)
    {
        return await _context.PointOfSalePaymentMethods
            .Include(pospm => pospm.PaymentMethod)
            .AnyAsync(pospm => 
                pospm.PointOfSaleId == pointOfSaleId && 
                pospm.PaymentMethodId == paymentMethodId &&
                pospm.IsActive &&
                pospm.PaymentMethod.IsActive);
    }

    /// <inheritdoc/>
    public async Task<int> GetActiveAssignmentCountAsync(Guid paymentMethodId)
    {
        return await _context.PointOfSalePaymentMethods
            .CountAsync(pospm => pospm.PaymentMethodId == paymentMethodId && pospm.IsActive);
    }
}
