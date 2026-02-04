namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents an optional photo attached to a return.
/// Photos document the condition of returned items.
/// </summary>
public class ReturnPhoto : BaseEntity
{
    /// <summary>
    /// The return this photo belongs to.
    /// </summary>
    public Guid ReturnId { get; set; }

    /// <summary>
    /// Path to the photo file in storage (relative or absolute depending on storage provider).
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Original or sanitized file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// File size in bytes.
    /// Maximum 2MB after compression.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., image/jpeg).
    /// All photos are stored as JPEG after compression.
    /// </summary>
    public required string MimeType { get; set; }

    // Navigation property

    /// <summary>
    /// Navigation property for the associated return.
    /// </summary>
    public virtual Return Return { get; set; } = null!;

    /// <summary>
    /// Validates that the file size does not exceed 2MB.
    /// </summary>
    /// <returns>True if file size is valid, false otherwise.</returns>
    public bool IsFileSizeValid() => FileSize <= 2 * 1024 * 1024; // 2MB in bytes

    /// <summary>
    /// Validates that the MIME type is image/jpeg.
    /// </summary>
    /// <returns>True if MIME type is valid, false otherwise.</returns>
    public bool IsMimeTypeValid() => MimeType == "image/jpeg";
}
