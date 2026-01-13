using JoiabagurPV.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for image compression operations using SixLabors.ImageSharp.
/// </summary>
public class ImageCompressionService : IImageCompressionService
{
    /// <inheritdoc/>
    public async Task<byte[]> CompressImageAsync(
        byte[] imageBytes, 
        int quality = 80, 
        long maxSizeBytes = 2 * 1024 * 1024, 
        int maxDimension = 1920)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            throw new ArgumentException("Image bytes cannot be null or empty.", nameof(imageBytes));
        }

        if (quality < 1 || quality > 100)
        {
            throw new ArgumentException("Quality must be between 1 and 100.", nameof(quality));
        }

        try
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(inputStream);

            // Resize if image exceeds maximum dimension
            if (image.Width > maxDimension || image.Height > maxDimension)
            {
                var options = new ResizeOptions
                {
                    Size = new Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max, // Maintain aspect ratio
                    Position = AnchorPositionMode.Center,
                    Sampler = KnownResamplers.Lanczos3 // High quality resampling
                };

                image.Mutate(x => x.Resize(options));
            }

            // Compress to JPEG
            using var outputStream = new MemoryStream();
            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(outputStream, encoder);
            var compressedBytes = outputStream.ToArray();

            // Validate output size
            if (compressedBytes.Length > maxSizeBytes)
            {
                throw new ArgumentException(
                    $"Image size ({compressedBytes.Length} bytes) exceeds maximum allowed size ({maxSizeBytes} bytes) even after compression.");
            }

            return compressedBytes;
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("Invalid or unsupported image format.");
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new InvalidOperationException("Failed to compress image.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(
        byte[] imageBytes, 
        long maxSizeBytes = 2 * 1024 * 1024)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return (false, "Image data is required.");
        }

        if (imageBytes.Length > maxSizeBytes)
        {
            return (false, $"Image size ({imageBytes.Length} bytes) exceeds maximum allowed size ({maxSizeBytes} bytes).");
        }

        try
        {
            using var inputStream = new MemoryStream(imageBytes);
            var imageInfo = await Image.IdentifyAsync(inputStream);

            if (imageInfo == null)
            {
                return (false, "Unable to identify image format.");
            }

            // Check minimum dimensions (at least 200x200 pixels)
            if (imageInfo.Width < 200 || imageInfo.Height < 200)
            {
                return (false, $"Image dimensions ({imageInfo.Width}x{imageInfo.Height}) are too small. Minimum 200x200 pixels required.");
            }

            // Check aspect ratio (max 5:1)
            var aspectRatio = (double)Math.Max(imageInfo.Width, imageInfo.Height) / 
                             Math.Min(imageInfo.Width, imageInfo.Height);
            
            if (aspectRatio > 5.0)
            {
                return (false, $"Image aspect ratio ({aspectRatio:F2}:1) exceeds maximum allowed (5:1).");
            }

            return (true, null);
        }
        catch (UnknownImageFormatException)
        {
            return (false, "Invalid or unsupported image format.");
        }
        catch (Exception ex)
        {
            return (false, $"Image validation failed: {ex.Message}");
        }
    }
}
