using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ProductComponent entity operations.
/// </summary>
public interface IProductComponentRepository : IRepository<ProductComponent>
{
    /// <summary>
    /// Checks if a component description already exists (case-insensitive).
    /// </summary>
    /// <param name="description">The description to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates).</param>
    /// <returns>True if description exists, false otherwise.</returns>
    Task<bool> DescriptionExistsAsync(string description, Guid? excludeId = null);

    /// <summary>
    /// Searches active components by description (partial, case-insensitive).
    /// </summary>
    /// <param name="query">Search query string.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>List of matching active components.</returns>
    Task<List<ProductComponent>> SearchActiveAsync(string query, int maxResults = 20);
}
