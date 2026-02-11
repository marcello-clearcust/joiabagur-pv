using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ComponentTemplate entity operations.
/// </summary>
public class ComponentTemplateRepository : Repository<ComponentTemplate>, IComponentTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public ComponentTemplateRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<ComponentTemplate?> GetWithItemsAsync(Guid id)
    {
        return await _context.ComponentTemplates
            .Include(t => t.Items)
                .ThenInclude(i => i.Component)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc/>
    public async Task<List<ComponentTemplate>> GetAllWithItemsAsync()
    {
        return await _context.ComponentTemplates
            .Include(t => t.Items)
                .ThenInclude(i => i.Component)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
