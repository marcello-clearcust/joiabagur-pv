using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for product management operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive products.</param>
    /// <returns>List of product DTOs.</returns>
    Task<List<ProductDto>> GetAllAsync(bool includeInactive = true);

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The product DTO if found.</returns>
    Task<ProductDto?> GetByIdAsync(Guid productId);

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <returns>The product DTO if found.</returns>
    Task<ProductDto?> GetBySkuAsync(string sku);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <returns>The created product DTO.</returns>
    Task<ProductDto> CreateAsync(CreateProductRequest request);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The product ID to update.</param>
    /// <param name="request">The update product request.</param>
    /// <returns>The updated product DTO.</returns>
    Task<ProductDto> UpdateAsync(Guid productId, UpdateProductRequest request);

    /// <summary>
    /// Soft deletes a product (sets IsActive to false).
    /// </summary>
    /// <param name="productId">The product ID.</param>
    Task DeactivateAsync(Guid productId);

    /// <summary>
    /// Reactivates a soft-deleted product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    Task ActivateAsync(Guid productId);
}



