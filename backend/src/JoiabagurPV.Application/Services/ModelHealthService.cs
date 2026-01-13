using JoiabagurPV.Application.DTOs.ImageRecognition;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for calculating model health metrics and alert levels.
/// </summary>
public class ModelHealthService : IModelHealthService
{
    private readonly IModelMetadataRepository _modelMetadataRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductPhotoRepository _productPhotoRepository;

    public ModelHealthService(
        IModelMetadataRepository modelMetadataRepository,
        IProductRepository productRepository,
        IProductPhotoRepository productPhotoRepository)
    {
        _modelMetadataRepository = modelMetadataRepository ?? throw new ArgumentNullException(nameof(modelMetadataRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _productPhotoRepository = productPhotoRepository ?? throw new ArgumentNullException(nameof(productPhotoRepository));
    }

    /// <inheritdoc/>
    public async Task<ModelHealthDto> GetModelHealthAsync()
    {
        var activeModel = await _modelMetadataRepository.GetActiveModelAsync();

        var catalogMetrics = await CalculateCatalogMetricsAsync(activeModel);
        var photoMetrics = await CalculatePhotoMetricsAsync(activeModel);

        var alertLevel = DetermineAlertLevel(activeModel, catalogMetrics, photoMetrics);

        return new ModelHealthDto
        {
            CurrentVersion = activeModel?.Version,
            LastTrainedAt = activeModel?.TrainedAt,
            DaysSinceTraining = activeModel != null 
                ? (int)(DateTime.UtcNow - activeModel.TrainedAt).TotalDays 
                : null,
            AlertLevel = alertLevel.Level,
            AlertMessage = alertLevel.Message,
            CatalogMetrics = catalogMetrics,
            PhotoMetrics = photoMetrics,
            PrecisionMetrics = null // Phase 2
        };
    }

    /// <inheritdoc/>
    public async Task<bool> ShouldRetrainAsync()
    {
        var health = await GetModelHealthAsync();
        return health.AlertLevel == "CRITICAL" || health.AlertLevel == "HIGH";
    }

    private async Task<CatalogMetricsDto> CalculateCatalogMetricsAsync(ModelMetadata? activeModel)
    {
        var allProducts = await _productRepository.GetAll().ToListAsync();
        var totalProducts = allProducts.Count;

        var productsWithPhotos = await _productPhotoRepository
            .GetAll()
            .Select(pp => pp.ProductId)
            .Distinct()
            .CountAsync();

        var productsWithoutPhotos = totalProducts - productsWithPhotos;

        var newProductsSinceTraining = 0;
        if (activeModel != null)
        {
            newProductsSinceTraining = allProducts
                .Count(p => p.CreatedAt > activeModel.TrainedAt);
        }

        var newProductsPercentage = totalProducts > 0 
            ? (newProductsSinceTraining / (double)totalProducts) * 100 
            : 0;

        return new CatalogMetricsDto
        {
            TotalProducts = totalProducts,
            ProductsWithPhotos = productsWithPhotos,
            ProductsWithoutPhotos = productsWithoutPhotos,
            NewProductsSinceTraining = newProductsSinceTraining,
            NewProductsPercentage = newProductsPercentage
        };
    }

    private async Task<PhotoMetricsDto> CalculatePhotoMetricsAsync(ModelMetadata? activeModel)
    {
        var totalPhotos = await _productPhotoRepository.GetAll().CountAsync();

        var photosAddedSinceTraining = 0;
        if (activeModel != null)
        {
            photosAddedSinceTraining = await _productPhotoRepository
                .GetAll()
                .CountAsync(pp => pp.CreatedAt > activeModel.TrainedAt);
        }

        // Note: We don't track deleted photos in MVP, so deletedCount is 0
        var photosDeletedSinceTraining = 0;

        var netChangePercentage = activeModel != null && activeModel.TotalPhotosUsed > 0
            ? ((photosAddedSinceTraining - photosDeletedSinceTraining) / (double)activeModel.TotalPhotosUsed) * 100
            : 0;

        return new PhotoMetricsDto
        {
            TotalPhotos = totalPhotos,
            PhotosAddedSinceTraining = photosAddedSinceTraining,
            PhotosDeletedSinceTraining = photosDeletedSinceTraining,
            NetChangePercentage = netChangePercentage
        };
    }

    private (string Level, string Message) DetermineAlertLevel(
        ModelMetadata? activeModel,
        CatalogMetricsDto catalogMetrics,
        PhotoMetricsDto photoMetrics)
    {
        // No model exists
        if (activeModel == null)
        {
            return ("CRITICAL", "No AI model exists. Please train an initial model.");
        }

        var daysSinceTraining = (int)(DateTime.UtcNow - activeModel.TrainedAt).TotalDays;

        // CRITICAL: Many new products without photos in model (≥20%)
        if (catalogMetrics.NewProductsPercentage >= 20)
        {
            return ("CRITICAL", $"{catalogMetrics.NewProductsSinceTraining} new products ({catalogMetrics.NewProductsPercentage:F1}%) added. Retrain immediately.");
        }

        // HIGH: Moderate new products (≥10%)
        if (catalogMetrics.NewProductsPercentage >= 10)
        {
            return ("HIGH", $"{catalogMetrics.NewProductsSinceTraining} new products ({catalogMetrics.NewProductsPercentage:F1}%) added. Retrain this week.");
        }

        // HIGH: Many photo changes (≥20%)
        if (Math.Abs(photoMetrics.NetChangePercentage) >= 20)
        {
            return ("HIGH", $"Photo catalog changed by {photoMetrics.NetChangePercentage:F1}%. Retrain this week.");
        }

        // HIGH: Model stale + changes (>30 days + ≥5% changes)
        if (daysSinceTraining > 30 && 
            (catalogMetrics.NewProductsPercentage >= 5 || Math.Abs(photoMetrics.NetChangePercentage) >= 10))
        {
            return ("HIGH", $"Model is {daysSinceTraining} days old with catalog changes. Retrain this week.");
        }

        // RECOMMENDED: Very stale model (>60 days)
        if (daysSinceTraining > 60)
        {
            return ("RECOMMENDED", $"Model is {daysSinceTraining} days old. Consider retraining.");
        }

        // OK: Model is up-to-date
        return ("OK", "Model is up-to-date.");
    }
}
