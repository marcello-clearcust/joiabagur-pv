using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Infrastructure.Services;

/// <summary>
/// Local file storage implementation for development environment.
/// Stores files in the local filesystem under a configured base path.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _basePath = configuration["FileStorage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/api/files";
        _logger = logger;

        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var folderPath = GetFolderPath(folder);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        _logger.LogInformation("File uploaded: {FileName} -> {StoredFileName}", fileName, uniqueFileName);

        return uniqueFileName;
    }

    /// <inheritdoc/>
    public Task<(Stream Stream, string ContentType)?> DownloadAsync(string storedFileName, string? folder = null)
    {
        var filePath = Path.Combine(GetFolderPath(folder), storedFileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<(Stream Stream, string ContentType)?>(null);
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var contentType = GetContentType(storedFileName);

        return Task.FromResult<(Stream Stream, string ContentType)?>((stream, contentType));
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string storedFileName, string? folder = null)
    {
        var filePath = Path.Combine(GetFolderPath(folder), storedFileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);
        _logger.LogInformation("File deleted: {StoredFileName}", storedFileName);

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        var path = folder != null ? $"{folder}/{storedFileName}" : storedFileName;
        var url = $"{_baseUrl}/{path}";
        return Task.FromResult(url);
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

    private string GetFolderPath(string? folder)
    {
        return folder != null ? Path.Combine(_basePath, folder) : _basePath;
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return $"{timestamp}_{uniqueId}{extension}";
    }

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
            _ => "application/octet-stream"
        };
    }
}



