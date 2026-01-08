using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Collection entity operations.
/// </summary>
public class CollectionRepository : Repository<Collection>, ICollectionRepository
{
    private readonly ApplicationDbContext _context;

    public CollectionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Collection?> GetByNameAsync(string name)
    {
        return await _context.Collections
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var lowerName = name.ToLower();

        if (excludeId.HasValue)
        {
            return await _context.Collections
                .AnyAsync(c => c.Name.ToLower() == lowerName && c.Id != excludeId.Value);
        }

        return await _context.Collections
            .AnyAsync(c => c.Name.ToLower() == lowerName);
    }

    /// <inheritdoc/>
    public async Task<List<Collection>> GetAllAsync()
    {
        return await _context.Collections
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Collection?> GetWithProductsAsync(Guid id)
    {
        return await _context.Collections
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Collection>> GetByNamesAsync(IEnumerable<string> names)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();

        var collections = await _context.Collections
            .Where(c => lowerNames.Contains(c.Name.ToLower()))
            .ToListAsync();

        return collections.ToDictionary(c => c.Name.ToLower(), c => c);
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<Collection> collections)
    {
        await _context.Collections.AddRangeAsync(collections);
    }
}




