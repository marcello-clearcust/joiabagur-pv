using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Data transfer object for inventory movement information.
/// </summary>
public class InventoryMovementDto
{
    /// <summary>
    /// The movement ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The inventory ID.
    /// </summary>
    public Guid InventoryId { get; set; }

    /// <summary>
    /// The product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The product SKU.
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// The product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The point of sale name.
    /// </summary>
    public string PointOfSaleName { get; set; } = string.Empty;

    /// <summary>
    /// The type of movement.
    /// </summary>
    public MovementType MovementType { get; set; }

    /// <summary>
    /// The movement type as string for display.
    /// </summary>
    public string MovementTypeName => MovementType.ToString();

    /// <summary>
    /// The change in quantity.
    /// </summary>
    public int QuantityChange { get; set; }

    /// <summary>
    /// The quantity before this movement.
    /// </summary>
    public int QuantityBefore { get; set; }

    /// <summary>
    /// The quantity after this movement.
    /// </summary>
    public int QuantityAfter { get; set; }

    /// <summary>
    /// The reason for this movement (for adjustments).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The user ID who performed this movement.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The username who performed this movement.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// When this movement occurred.
    /// </summary>
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// When this record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Paginated result for movement history queries.
/// </summary>
public class PaginatedMovementResult
{
    /// <summary>
    /// The movements on this page.
    /// </summary>
    public List<InventoryMovementDto> Items { get; set; } = new();

    /// <summary>
    /// Total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Filter criteria for movement history queries.
/// </summary>
public class MovementHistoryFilter
{
    /// <summary>
    /// Optional product ID filter.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Optional point of sale ID filter.
    /// </summary>
    public Guid? PointOfSaleId { get; set; }

    /// <summary>
    /// Start date filter (defaults to 30 days ago).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date filter (defaults to now).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Page number (1-based, defaults to 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (max 50, defaults to 50).
    /// </summary>
    public int PageSize { get; set; } = 50;
}

