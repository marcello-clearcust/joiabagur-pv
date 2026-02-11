using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ComponentTemplate entity operations.
/// </summary>
public interface IComponentTemplateRepository : IRepository<ComponentTemplate>
{
    /// <summary>
    /// Gets a template with its items and component details loaded.
    /// </summary>
    /// <param name="id">The template ID.</param>
    /// <returns>The template with items, or null if not found.</returns>
    Task<ComponentTemplate?> GetWithItemsAsync(Guid id);

    /// <summary>
    /// Gets all templates with item counts.
    /// </summary>
    /// <returns>List of all templates.</returns>
    Task<List<ComponentTemplate>> GetAllWithItemsAsync();
}
