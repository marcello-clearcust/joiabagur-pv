namespace JoiabagurPV.Application.DTOs.Products;

/// <summary>
/// Generic DTO for paginated responses.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PaginatedResultDto<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total count of all items matching the query (before pagination).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Page size (number of items per page).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Creates a paginated result from items and pagination info.
    /// </summary>
    public static PaginatedResultDto<T> Create(
        List<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}

/// <summary>
/// Request parameters for catalog queries.
/// </summary>
public class CatalogQueryParameters
{
    /// <summary>
    /// Page number (1-based). Defaults to 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 50. Max 100.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort field. Options: "name", "createdAt", "price". Defaults to "name".
    /// </summary>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction. "asc" or "desc". Defaults to "asc".
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Whether to include inactive products. Defaults to false.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}

