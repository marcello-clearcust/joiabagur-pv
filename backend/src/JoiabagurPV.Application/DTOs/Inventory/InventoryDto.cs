namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Data transfer object for inventory information.
/// </summary>
public class InventoryDto
{
    /// <summary>
    /// The inventory record ID.
    /// </summary>
    public Guid Id { get; set; }

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
    /// The current quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Whether this inventory assignment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the inventory was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// When the inventory was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Aggregated stock view for centralized display.
/// </summary>
public class CentralizedStockDto
{
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
    /// Total quantity across all points of sale.
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Breakdown by point of sale.
    /// </summary>
    public List<PointOfSaleStockDto> Breakdown { get; set; } = new();
}

/// <summary>
/// Stock quantity at a specific point of sale.
/// </summary>
public class PointOfSaleStockDto
{
    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The point of sale name.
    /// </summary>
    public string PointOfSaleName { get; set; } = string.Empty;

    /// <summary>
    /// The quantity at this point of sale.
    /// </summary>
    public int Quantity { get; set; }
}

/// <summary>
/// Paginated result for inventory queries.
/// </summary>
public class PaginatedInventoryResult
{
    /// <summary>
    /// The inventory items on this page.
    /// </summary>
    public List<InventoryDto> Items { get; set; } = new();

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
/// Paginated result for centralized stock queries.
/// </summary>
public class PaginatedCentralizedStockResult
{
    /// <summary>
    /// The stock items on this page.
    /// </summary>
    public List<CentralizedStockDto> Items { get; set; } = new();

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

