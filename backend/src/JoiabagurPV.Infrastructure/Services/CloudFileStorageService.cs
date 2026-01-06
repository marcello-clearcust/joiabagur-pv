using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Infrastructure.Services;

/// <summary>
/// Cloud file storage implementation for production environment.
/// This is a placeholder that delegates to LocalFileStorageService.
/// TODO: Implement AWS S3 or Azure Blob Storage when deploying to production.
/// </summary>
public class CloudFileStorageService : IFileStorageService
{
    private readonly LocalFileStorageService _localService;
    private readonly ILogger<CloudFileStorageService> _logger;

    public CloudFileStorageService(IConfiguration configuration, ILogger<CloudFileStorageService> logger)
    {
        _logger = logger;
        // For now, delegate to local storage. In production, this would use S3/Azure Blob.
        _localService = new LocalFileStorageService(configuration, 
            new LoggerFactory().CreateLogger<LocalFileStorageService>());
        
        _logger.LogWarning("CloudFileStorageService is using local storage fallback. Configure S3/Azure Blob for production.");
    }

    /// <inheritdoc/>
    public Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null)
    {
        // TODO: Implement cloud storage upload
        // For AWS S3:
        // var request = new PutObjectRequest { BucketName = _bucketName, Key = key, InputStream = stream, ContentType = contentType };
        // await _s3Client.PutObjectAsync(request);
        return _localService.UploadAsync(stream, fileName, contentType, folder);
    }

    /// <inheritdoc/>
    public Task<(Stream Stream, string ContentType)?> DownloadAsync(string storedFileName, string? folder = null)
    {
        // TODO: Implement cloud storage download
        return _localService.DownloadAsync(storedFileName, folder);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string storedFileName, string? folder = null)
    {
        // TODO: Implement cloud storage delete
        return _localService.DeleteAsync(storedFileName, folder);
    }

    /// <inheritdoc/>
    public Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        // TODO: Implement pre-signed URL generation for cloud storage
        return _localService.GetUrlAsync(storedFileName, folder);
    }

    /// <inheritdoc/>
    public (bool IsValid, string? ErrorMessage) ValidateFile(
        string fileName,
        string contentType,
        long sizeBytes,
        string[]? allowedExtensions = null,
        long? maxSizeBytes = null)
    {
        return _localService.ValidateFile(fileName, contentType, sizeBytes, allowedExtensions, maxSizeBytes);
    }
}



