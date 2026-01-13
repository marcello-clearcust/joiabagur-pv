namespace JoiabagurPV.Application.DTOs.Sales;

/// <summary>
/// Response DTO for sales history with pagination.
/// </summary>
public class SalesHistoryResponse
{
    /// <summary>
    /// List of sales.
    /// </summary>
    public List<SaleDto> Sales { get; set; } = new();

    /// <summary>
    /// Total count of sales matching the filters.
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
    /// Whether there are more pages.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there are previous pages.
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
