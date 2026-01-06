using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for product management operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICollectionRepository collectionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _collectionRepository = collectionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<ProductDto>> GetAllAsync(bool includeInactive = true)
    {
        var products = await _productRepository.GetAllAsync(includeInactive);
        return products.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<ProductDto?> GetByIdAsync(Guid productId)
    {
        var product = await _productRepository.GetWithPhotosAsync(productId);
        return product != null ? MapToDto(product) : null;
    }

    /// <inheritdoc/>
    public async Task<ProductDto?> GetBySkuAsync(string sku)
    {
        var normalizedSku = (sku ?? string.Empty).Trim().ToUpperInvariant();
        var product = await _productRepository.GetBySkuAsync(normalizedSku);
        return product != null ? MapToDto(product) : null;
    }

    /// <inheritdoc/>
    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var normalizedSku = (request.SKU ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedName = (request.Name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedSku))
        {
            throw new DomainException("El SKU es requerido");
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new DomainException("El nombre es requerido");
        }

        // Validate SKU uniqueness
        if (await _productRepository.SkuExistsAsync(normalizedSku))
        {
            throw new DomainException($"A product with SKU '{normalizedSku}' already exists.");
        }

        // Validate price
        if (request.Price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        // Validate collection if provided
        if (request.CollectionId.HasValue)
        {
            var collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
            if (collection == null)
            {
                throw new DomainException($"Collection with ID '{request.CollectionId}' not found.");
            }

            // Ensure CollectionName is available in the response DTO
            // (MapToDto uses product.Collection?.Name)
            // We'll attach the loaded entity to the new product below.
        }

        var product = new Product
        {
            SKU = normalizedSku,
            Name = normalizedName,
            Description = request.Description,
            Price = request.Price,
            CollectionId = request.CollectionId,
            IsActive = true
        };

        if (request.CollectionId.HasValue)
        {
            product.Collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
        }

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product created: {SKU} - {Name}", product.SKU, product.Name);

        return MapToDto(product);
    }

    /// <inheritdoc/>
    public async Task<ProductDto> UpdateAsync(Guid productId, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new DomainException($"Product with ID '{productId}' not found.");
        }

        var normalizedName = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new DomainException("El nombre es requerido");
        }

        // Validate price
        if (request.Price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        // Validate collection if provided
        if (request.CollectionId.HasValue)
        {
            if (!await _collectionRepository.ExistsAsync(request.CollectionId.Value))
            {
                throw new DomainException($"Collection with ID '{request.CollectionId}' not found.");
            }
        }

        product.Name = normalizedName;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CollectionId = request.CollectionId;
        product.IsActive = request.IsActive;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product updated: {SKU} - {Name}", product.SKU, product.Name);

        return MapToDto(product);
    }

    /// <inheritdoc/>
    public async Task DeactivateAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new DomainException($"Product with ID '{productId}' not found.");
        }

        product.IsActive = false;
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product deactivated: {SKU}", product.SKU);
    }

    /// <inheritdoc/>
    public async Task ActivateAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new DomainException($"Product with ID '{productId}' not found.");
        }

        product.IsActive = true;
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product activated: {SKU}", product.SKU);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CollectionId = product.CollectionId,
            CollectionName = product.Collection?.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Photos = product.Photos?.Select(p => new ProductPhotoDto
            {
                Id = p.Id,
                FileName = p.FileName,
                DisplayOrder = p.DisplayOrder,
                IsPrimary = p.IsPrimary
            }).ToList() ?? new List<ProductPhotoDto>()
        };
    }
}



