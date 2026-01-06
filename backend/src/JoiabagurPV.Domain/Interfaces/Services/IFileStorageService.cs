namespace JoiabagurPV.Domain.Interfaces.Services;

/// <summary>
/// Service interface for file storage operations.
/// Supports local development and cloud production environments.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="stream">The file stream to upload.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="folder">Optional folder/container path.</param>
    /// <returns>The stored file name (unique identifier).</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="storedFileName">The stored file name.</param>
    /// <param name="folder">Optional folder/container path.</param>
    /// <returns>The file stream and content type.</returns>
    Task<(Stream Stream, string ContentType)?> DownloadAsync(string storedFileName, string? folder = null);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="storedFileName">The stored file name.</param>
    /// <param name="folder">Optional folder/container path.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string storedFileName, string? folder = null);

    /// <summary>
    /// Gets the URL to access a file.
    /// </summary>
    /// <param name="storedFileName">The stored file name.</param>
    /// <param name="folder">Optional folder/container path.</param>
    /// <returns>The URL to access the file.</returns>
    Task<string> GetUrlAsync(string storedFileName, string? folder = null);

    /// <summary>
    /// Validates a file before upload.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="sizeBytes">The file size in bytes.</param>
    /// <param name="allowedExtensions">Optional list of allowed extensions (e.g., ".jpg", ".png").</param>
    /// <param name="maxSizeBytes">Optional maximum file size in bytes.</param>
    /// <returns>Validation result with error message if invalid.</returns>
    (bool IsValid, string? ErrorMessage) ValidateFile(
        string fileName,
        string contentType,
        long sizeBytes,
        string[]? allowedExtensions = null,
        long? maxSizeBytes = null);
}



