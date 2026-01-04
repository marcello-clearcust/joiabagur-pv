using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for PaymentMethod entity operations.
/// </summary>
public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentMethodRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaymentMethod?> GetByCodeAsync(string code)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Code == code.ToUpperInvariant());
    }

    /// <inheritdoc/>
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var upperCode = code.ToUpperInvariant();
        
        if (excludeId.HasValue)
        {
            return await _context.PaymentMethods
                .AnyAsync(pm => pm.Code == upperCode && pm.Id != excludeId.Value);
        }

        return await _context.PaymentMethods
            .AnyAsync(pm => pm.Code == upperCode);
    }

    /// <inheritdoc/>
    public async Task<List<PaymentMethod>> GetAllAsync(bool includeInactive = true)
    {
        var query = _context.PaymentMethods.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(pm => pm.IsActive);
        }

        return await query
            .OrderBy(pm => pm.Name)
            .ToListAsync();
    }
}
