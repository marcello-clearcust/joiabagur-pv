namespace JoiabagurPV.Application.DTOs.Returns;

/// <summary>
/// Result DTO for return creation.
/// </summary>
public class CreateReturnResult
{
    /// <summary>
    /// Whether the return was created successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if creation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The created return if successful.
    /// </summary>
    public ReturnDto? Return { get; set; }

    /// <summary>
    /// Updated stock quantity after the return.
    /// </summary>
    public int? NewStockQuantity { get; set; }
}
