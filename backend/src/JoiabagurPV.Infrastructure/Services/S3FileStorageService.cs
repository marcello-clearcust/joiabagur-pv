using Amazon.S3;
using Amazon.S3.Model;
using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace JoiabagurPV.Infrastructure.Services;

/// <summary>
/// AWS S3 file storage implementation for production environment.
/// Uses pre-signed URLs for secure file access.
/// </summary>
public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly int _presignedUrlExpirationMinutes;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _bucketName = configuration["Aws:S3:BucketName"]
            ?? throw new ArgumentException("Aws:S3:BucketName configuration is required");
        _presignedUrlExpirationMinutes = int.Parse(
            configuration["Aws:S3:PresignedUrlExpirationMinutes"] ?? "60");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var key = BuildKey(folder, uniqueFileName);

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("File uploaded to S3: {Key}", key);

            return uniqueFileName;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 upload failed for {Key}: {ErrorCode} - {Message}", 
                key, ex.ErrorCode, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<(Stream Stream, string ContentType)?> DownloadAsync(string storedFileName, string? folder = null)
    {
        var key = BuildKey(folder, storedFileName);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            
            // Copy to memory stream so we can return it safely
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return (memoryStream, response.Headers.ContentType ?? GetContentType(storedFileName));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found in S3: {Key}", key);
            return null;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 download failed for {Key}: {ErrorCode} - {Message}", 
                key, ex.ErrorCode, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string storedFileName, string? folder = null)
    {
        var key = BuildKey(folder, storedFileName);

        try
        {
            // Check if the object exists first
            try
            {
                await _s3Client.GetObjectMetadataAsync(_bucketName, key);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File not found for deletion in S3: {Key}", key);
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("File deleted from S3: {Key}", key);

            return true;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 delete failed for {Key}: {ErrorCode} - {Message}", 
                key, ex.ErrorCode, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        var key = BuildKey(folder, storedFileName);

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(_presignedUrlExpirationMinutes),
                Verb = HttpVerb.GET
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogDebug("Generated pre-signed URL for {Key}, expires in {Minutes} minutes", 
                key, _presignedUrlExpirationMinutes);

            return Task.FromResult(url);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Failed to generate pre-signed URL for {Key}: {ErrorCode} - {Message}", 
                key, ex.ErrorCode, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public (bool IsValid, string? ErrorMessage) ValidateFile(
        string fileName,
        string contentType,
        long sizeBytes,
        string[]? allowedExtensions = null,
        long? maxSizeBytes = null)
    {
        // Check file extension
        if (allowedExtensions != null && allowedExtensions.Length > 0)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return (false, $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
            }
        }

        // Check file size
        if (maxSizeBytes.HasValue && sizeBytes > maxSizeBytes.Value)
        {
            var maxSizeMB = maxSizeBytes.Value / (1024 * 1024);
            return (false, $"File size exceeds maximum allowed size of {maxSizeMB} MB");
        }

        return (true, null);
    }

    /// <summary>
    /// Builds the S3 object key from folder and filename.
    /// </summary>
    private static string BuildKey(string? folder, string fileName)
    {
        return folder != null ? $"{folder}/{fileName}" : fileName;
    }

    /// <summary>
    /// Generates a unique filename using timestamp and GUID.
    /// </summary>
    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return $"{timestamp}_{uniqueId}{extension}";
    }

    /// <summary>
    /// Gets the content type based on file extension.
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".pdf" => "application/pdf",
            ".json" => "application/json",
            ".tflite" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}
