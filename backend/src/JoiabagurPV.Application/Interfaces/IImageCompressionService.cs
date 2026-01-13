namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for image compression operations.
/// </summary>
public interface IImageCompressionService
{
    /// <summary>
    /// Compresses an image to JPEG format with specified quality and size constraints.
    /// </summary>
    /// <param name="imageBytes">The original image bytes.</param>
    /// <param name="quality">JPEG quality (1-100). Default is 80.</param>
    /// <param name="maxSizeBytes">Maximum file size in bytes. Default is 2MB.</param>
    /// <param name="maxDimension">Maximum width or height in pixels. Default is 1920.</param>
    /// <returns>Compressed image bytes in JPEG format.</returns>
    /// <exception cref="ArgumentException">Thrown when image cannot be compressed within constraints.</exception>
    Task<byte[]> CompressImageAsync(
        byte[] imageBytes, 
        int quality = 80, 
        long maxSizeBytes = 2 * 1024 * 1024, 
        int maxDimension = 1920);

    /// <summary>
    /// Validates that an image meets the size and format requirements.
    /// </summary>
    /// <param name="imageBytes">The image bytes to validate.</param>
    /// <param name="maxSizeBytes">Maximum file size in bytes.</param>
    /// <returns>True if valid, false otherwise.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(
        byte[] imageBytes, 
        long maxSizeBytes = 2 * 1024 * 1024);
}
