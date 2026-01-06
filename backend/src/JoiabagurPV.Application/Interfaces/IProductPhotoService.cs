using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for product photo management operations.
/// </summary>
public interface IProductPhotoService
{
    /// <summary>
    /// Uploads a photo for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="stream">The photo file stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <returns>The uploaded photo DTO.</returns>
    Task<ProductPhotoDto> UploadPhotoAsync(Guid productId, Stream stream, string fileName, string contentType);

    /// <summary>
    /// Gets all photos for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>List of photo DTOs ordered by DisplayOrder.</returns>
    Task<List<ProductPhotoDto>> GetProductPhotosAsync(Guid productId);

    /// <summary>
    /// Sets a photo as the primary photo for a product.
    /// </summary>
    /// <param name="photoId">The photo ID to set as primary.</param>
    Task SetPrimaryPhotoAsync(Guid photoId);

    /// <summary>
    /// Deletes a photo.
    /// </summary>
    /// <param name="photoId">The photo ID to delete.</param>
    Task DeletePhotoAsync(Guid photoId);

    /// <summary>
    /// Updates the display order of photos for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="photoOrders">Dictionary mapping photo IDs to their new display order.</param>
    Task UpdateDisplayOrderAsync(Guid productId, Dictionary<Guid, int> photoOrders);
}



