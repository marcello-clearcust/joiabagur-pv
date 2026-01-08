using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ProductPhoto entity operations.
/// </summary>
public interface IProductPhotoRepository : IRepository<ProductPhoto>
{
    /// <summary>
    /// Gets all photos for a specific product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>List of photos ordered by DisplayOrder.</returns>
    Task<List<ProductPhoto>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Gets the primary photo for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The primary photo if exists, null otherwise.</returns>
    Task<ProductPhoto?> GetPrimaryPhotoAsync(Guid productId);

    /// <summary>
    /// Updates the display order of photos for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="photoOrders">Dictionary mapping photo IDs to their new display order.</param>
    Task UpdateDisplayOrderAsync(Guid productId, Dictionary<Guid, int> photoOrders);

    /// <summary>
    /// Sets a photo as primary and unmarks any existing primary photo for the product.
    /// </summary>
    /// <param name="photoId">The photo ID to set as primary.</param>
    Task SetPrimaryPhotoAsync(Guid photoId);

    /// <summary>
    /// Gets the next available display order for a product's photos.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The next display order value.</returns>
    Task<int> GetNextDisplayOrderAsync(Guid productId);
}




