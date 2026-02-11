using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ProductComponent entity operations.
/// </summary>
public class ProductComponentRepository : Repository<ProductComponent>, IProductComponentRepository
{
    private readonly ApplicationDbContext _context;

    public ProductComponentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<bool> DescriptionExistsAsync(string description, Guid? excludeId = null)
    {
        var normalized = (description ?? string.Empty).Trim().ToUpperInvariant();
        var query = _context.ProductComponents
            .Where(c => c.Description.ToUpper() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ProductComponent>> SearchActiveAsync(string query, int maxResults = 20)
    {
        var normalized = (query ?? string.Empty).Trim().ToUpperInvariant();
        return await _context.ProductComponents
            .Where(c => c.IsActive && c.Description.ToUpper().Contains(normalized))
            .OrderBy(c => c.Description)
            .Take(maxResults)
            .ToListAsync();
    }
}
