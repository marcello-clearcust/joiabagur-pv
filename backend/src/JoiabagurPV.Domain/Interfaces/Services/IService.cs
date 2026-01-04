using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Services;

/// <summary>
/// Generic service interface for business logic operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IService<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    Task<TEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The created entity.</returns>
    Task<TEntity> CreateAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="entity">The updated entity data.</param>
    /// <returns>The updated entity.</returns>
    Task<TEntity> UpdateAsync(Guid id, TEntity entity);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(Guid id);
}