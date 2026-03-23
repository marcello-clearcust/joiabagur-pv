using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ProductPhotoEmbedding entity operations.
/// </summary>
public interface IProductPhotoEmbeddingRepository : IRepository<ProductPhotoEmbedding>
{
    /// <summary>
    /// Gets all stored embeddings.
    /// </summary>
    Task<List<ProductPhotoEmbedding>> GetAllAsync();

    /// <summary>
    /// Gets the embedding for a specific photo.
    /// </summary>
    Task<ProductPhotoEmbedding?> GetByPhotoIdAsync(Guid photoId);

    /// <summary>
    /// Gets all embeddings for a specific product.
    /// </summary>
    Task<List<ProductPhotoEmbedding>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Deletes the embedding associated with a specific photo.
    /// </summary>
    Task DeleteByPhotoIdAsync(Guid photoId);

    /// <summary>
    /// Deletes all stored embeddings.
    /// </summary>
    Task DeleteAllAsync();

    /// <summary>
    /// Gets the total count of stored embeddings.
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Gets the timestamp of the most recently updated embedding.
    /// </summary>
    Task<DateTime?> GetLastUpdatedAsync();
}
