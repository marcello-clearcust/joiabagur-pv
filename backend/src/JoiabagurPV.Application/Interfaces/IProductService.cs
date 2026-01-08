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
    /// Gets a paginated list of products for catalog display.
    /// Applies role-based filtering: administrators see all products,
    /// operators see only products with inventory at their assigned points of sale.
    /// </summary>
    /// <param name="parameters">Query parameters for pagination and sorting.</param>
    /// <param name="userId">The current user's ID for filtering.</param>
    /// <param name="isAdmin">Whether the user is an administrator.</param>
    /// <returns>Paginated result of product list DTOs.</returns>
    Task<PaginatedResultDto<ProductListDto>> GetProductsAsync(
        CatalogQueryParameters parameters,
        Guid? userId = null,
        bool isAdmin = true);

    /// <summary>
    /// Searches products by SKU (exact match) or name (partial match).
    /// Applies role-based filtering: administrators see all products,
    /// operators see only products with inventory at their assigned points of sale.
    /// </summary>
    /// <param name="query">Search query (SKU or name).</param>
    /// <param name="userId">The current user's ID for filtering.</param>
    /// <param name="isAdmin">Whether the user is an administrator.</param>
    /// <param name="maxResults">Maximum results to return. Default 50.</param>
    /// <returns>List of matching product list DTOs.</returns>
    Task<List<ProductListDto>> SearchProductsAsync(
        string query,
        Guid? userId = null,
        bool isAdmin = true,
        int maxResults = 50);

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




