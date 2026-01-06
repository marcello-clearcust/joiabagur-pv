using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Collection entity operations.
/// </summary>
public interface ICollectionRepository : IRepository<Collection>
{
    /// <summary>
    /// Gets a collection by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The collection if found, null otherwise.</returns>
    Task<Collection?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a collection name is already in use (case-insensitive).
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <returns>True if name exists, false otherwise.</returns>
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);

    /// <summary>
    /// Gets all collections.
    /// </summary>
    /// <returns>List of all collections.</returns>
    Task<List<Collection>> GetAllAsync();

    /// <summary>
    /// Gets a collection with its products loaded.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <returns>The collection with products, or null if not found.</returns>
    Task<Collection?> GetWithProductsAsync(Guid id);

    /// <summary>
    /// Gets multiple collections by their names (case-insensitive).
    /// </summary>
    /// <param name="names">Collection of names to search for.</param>
    /// <returns>Dictionary mapping names (lowercase) to collections.</returns>
    Task<Dictionary<string, Collection>> GetByNamesAsync(IEnumerable<string> names);

    /// <summary>
    /// Adds multiple collections in a batch.
    /// </summary>
    /// <param name="collections">The collections to add.</param>
    Task AddRangeAsync(IEnumerable<Collection> collections);
}



