using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ModelMetadata entity operations.
/// </summary>
public interface IModelMetadataRepository : IRepository<ModelMetadata>
{
    /// <summary>
    /// Gets the currently active model.
    /// </summary>
    /// <returns>The active model if exists, null otherwise.</returns>
    Task<ModelMetadata?> GetActiveModelAsync();

    /// <summary>
    /// Gets a model by its version identifier.
    /// </summary>
    /// <param name="version">The model version.</param>
    /// <returns>The model if found, null otherwise.</returns>
    Task<ModelMetadata?> GetByVersionAsync(string version);

    /// <summary>
    /// Gets all model versions ordered by training date descending.
    /// </summary>
    /// <returns>List of all model versions.</returns>
    Task<List<ModelMetadata>> GetAllVersionsAsync();

    /// <summary>
    /// Deactivates the currently active model.
    /// </summary>
    Task DeactivateCurrentModelAsync();

    /// <summary>
    /// Activates a specific model version.
    /// </summary>
    /// <param name="modelId">The model ID to activate.</param>
    Task ActivateModelAsync(Guid modelId);
}
