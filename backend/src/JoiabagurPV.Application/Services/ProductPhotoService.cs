using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for product photo management operations.
/// </summary>
public class ProductPhotoService : IProductPhotoService
{
    private readonly IProductPhotoRepository _photoRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductPhotoService> _logger;

    private const string PhotoFolder = "products";
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ProductPhotoService(
        IProductPhotoRepository photoRepository,
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<ProductPhotoService> logger)
    {
        _photoRepository = photoRepository;
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ProductPhotoDto> UploadPhotoAsync(
        Guid productId, 
        Stream stream, 
        string fileName, 
        string contentType)
    {
        // Validate product exists
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new DomainException($"Product with ID '{productId}' not found.");
        }

        // Validate file
        var validation = _fileStorageService.ValidateFile(
            fileName, 
            contentType, 
            stream.Length, 
            AllowedExtensions, 
            MaxFileSizeBytes);

        if (!validation.IsValid)
        {
            throw new DomainException(validation.ErrorMessage ?? "Invalid file.");
        }

        // Upload file to storage
        var storedFileName = await _fileStorageService.UploadAsync(
            stream, 
            fileName, 
            contentType, 
            PhotoFolder);

        // Get next display order
        var displayOrder = await _photoRepository.GetNextDisplayOrderAsync(productId);

        // Check if this should be the primary photo (first photo for the product)
        var existingPhotos = await _photoRepository.GetByProductIdAsync(productId);
        var isPrimary = existingPhotos.Count == 0;

        // Create photo entity
        var photo = new ProductPhoto
        {
            ProductId = productId,
            FileName = storedFileName,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary
        };

        await _photoRepository.AddAsync(photo);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Photo uploaded for product {ProductId}: {FileName}", 
            productId, 
            storedFileName);

        return await MapToDtoAsync(photo);
    }

    /// <inheritdoc/>
    public async Task<List<ProductPhotoDto>> GetProductPhotosAsync(Guid productId)
    {
        var photos = await _photoRepository.GetByProductIdAsync(productId);
        var photoDtos = new List<ProductPhotoDto>();

        foreach (var photo in photos)
        {
            photoDtos.Add(await MapToDtoAsync(photo));
        }

        return photoDtos;
    }

    /// <inheritdoc/>
    public async Task SetPrimaryPhotoAsync(Guid photoId)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            throw new DomainException($"Photo with ID '{photoId}' not found.");
        }

        // Use repository method to handle primary photo logic
        await _photoRepository.SetPrimaryPhotoAsync(photoId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Photo {PhotoId} set as primary for product {ProductId}", 
            photoId, 
            photo.ProductId);
    }

    /// <inheritdoc/>
    public async Task DeletePhotoAsync(Guid photoId)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            throw new DomainException($"Photo with ID '{photoId}' not found.");
        }

        var productId = photo.ProductId;
        var wasPrimary = photo.IsPrimary;

        // Delete file from storage
        var deleted = await _fileStorageService.DeleteAsync(photo.FileName, PhotoFolder);
        if (!deleted)
        {
            _logger.LogWarning(
                "File {FileName} not found in storage, continuing with database deletion", 
                photo.FileName);
        }

        // Delete photo entity
        await _photoRepository.DeleteAsync(photo.Id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Photo {PhotoId} deleted for product {ProductId}", 
            photoId, 
            productId);

        // If this was the primary photo, set another photo as primary
        if (wasPrimary)
        {
            var remainingPhotos = await _photoRepository.GetByProductIdAsync(productId);
            if (remainingPhotos.Count > 0)
            {
                var newPrimary = remainingPhotos.OrderBy(p => p.DisplayOrder).First();
                await _photoRepository.SetPrimaryPhotoAsync(newPrimary.Id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Photo {PhotoId} automatically set as new primary for product {ProductId}", 
                    newPrimary.Id, 
                    productId);
            }
        }
    }

    /// <inheritdoc/>
    public async Task UpdateDisplayOrderAsync(Guid productId, Dictionary<Guid, int> photoOrders)
    {
        // Validate product exists
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new DomainException($"Product with ID '{productId}' not found.");
        }

        // Validate all photos belong to the product
        var photos = await _photoRepository.GetByProductIdAsync(productId);
        var photoIds = photos.Select(p => p.Id).ToHashSet();

        foreach (var photoId in photoOrders.Keys)
        {
            if (!photoIds.Contains(photoId))
            {
                throw new DomainException(
                    $"Photo with ID '{photoId}' does not belong to product '{productId}'.");
            }
        }

        await _photoRepository.UpdateDisplayOrderAsync(productId, photoOrders);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Display order updated for {Count} photos of product {ProductId}", 
            photoOrders.Count, 
            productId);
    }

    private async Task<ProductPhotoDto> MapToDtoAsync(ProductPhoto photo)
    {
        var url = await _fileStorageService.GetUrlAsync(photo.FileName, PhotoFolder);

        return new ProductPhotoDto
        {
            Id = photo.Id,
            ProductId = photo.ProductId,
            FileName = photo.FileName,
            Url = url,
            DisplayOrder = photo.DisplayOrder,
            IsPrimary = photo.IsPrimary,
            CreatedAt = photo.CreatedAt,
            UpdatedAt = photo.UpdatedAt
        };
    }
}



