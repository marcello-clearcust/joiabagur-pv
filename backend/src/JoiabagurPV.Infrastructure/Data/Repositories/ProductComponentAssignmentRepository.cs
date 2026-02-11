using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ProductComponentAssignment entity operations.
/// </summary>
public class ProductComponentAssignmentRepository : Repository<ProductComponentAssignment>, IProductComponentAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public ProductComponentAssignmentRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<ProductComponentAssignment>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductComponentAssignments
            .Include(a => a.Component)
            .Where(a => a.ProductId == productId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task RemoveAllByProductIdAsync(Guid productId)
    {
        var assignments = await _context.ProductComponentAssignments
            .Where(a => a.ProductId == productId)
            .ToListAsync();

        _context.ProductComponentAssignments.RemoveRange(assignments);
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<ProductComponentAssignment> assignments)
    {
        await _context.ProductComponentAssignments.AddRangeAsync(assignments);
    }

    /// <inheritdoc/>
    public async Task<bool> HasAssignmentsAsync(Guid productId)
    {
        return await _context.ProductComponentAssignments
            .AnyAsync(a => a.ProductId == productId);
    }
}
