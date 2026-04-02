namespace JoiabagurPV.Domain.Common;

/// <summary>
/// Shared pagination limits used across services and repositories.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// Maximum items per page for standard paginated list endpoints (sales history, returns history, etc.).
    /// </summary>
    public const int MaxPageSize = 1000;
}
