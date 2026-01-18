namespace JoiabagurPV.Application.DTOs.Returns;

/// <summary>
/// Response DTO for returns history.
/// </summary>
public class ReturnsHistoryResponse
{
    /// <summary>
    /// List of returns.
    /// </summary>
    public List<ReturnDto> Returns { get; set; } = new();

    /// <summary>
    /// Total number of returns matching the filter.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
