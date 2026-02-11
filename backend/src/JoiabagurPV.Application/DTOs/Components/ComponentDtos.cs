namespace JoiabagurPV.Application.DTOs.Components;

/// <summary>
/// Response DTO for a component from the master table.
/// </summary>
public class ComponentResponseDto
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a new component.
/// </summary>
public class CreateComponentRequest
{
    public required string Description { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SalePrice { get; set; }
}

/// <summary>
/// Request DTO for updating an existing component.
/// </summary>
public class UpdateComponentRequest
{
    public required string Description { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Query parameters for listing components with filters and pagination.
/// </summary>
public class ComponentQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// DTO for a component assignment on a product.
/// </summary>
public class ComponentAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ComponentId { get; set; }
    public required string ComponentDescription { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int DisplayOrder { get; set; }
    /// <summary>
    /// Master table CostPrice for reference (for sync feature).
    /// </summary>
    public decimal? MasterCostPrice { get; set; }
    /// <summary>
    /// Master table SalePrice for reference (for sync feature).
    /// </summary>
    public decimal? MasterSalePrice { get; set; }
}

/// <summary>
/// Request DTO for saving component assignments on a product (replaces all).
/// </summary>
public class SaveComponentAssignmentsRequest
{
    public List<ComponentAssignmentItemRequest> Assignments { get; set; } = new();
}

/// <summary>
/// Individual assignment item in a save request.
/// </summary>
public class ComponentAssignmentItemRequest
{
    public Guid ComponentId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for price sync preview (before/after prices).
/// </summary>
public class PriceSyncPreviewDto
{
    public List<PriceSyncItemDto> Items { get; set; } = new();
}

/// <summary>
/// Individual item in the price sync preview.
/// </summary>
public class PriceSyncItemDto
{
    public Guid ComponentId { get; set; }
    public required string ComponentDescription { get; set; }
    public decimal CurrentCostPrice { get; set; }
    public decimal CurrentSalePrice { get; set; }
    public decimal? NewCostPrice { get; set; }
    public decimal? NewSalePrice { get; set; }
    public bool WillBeUpdated { get; set; }
}

/// <summary>
/// Response DTO for a component template.
/// </summary>
public class ComponentTemplateDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<ComponentTemplateItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for a template item (component + quantity).
/// </summary>
public class ComponentTemplateItemDto
{
    public Guid ComponentId { get; set; }
    public required string ComponentDescription { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Request DTO for creating/updating a template.
/// </summary>
public class SaveComponentTemplateRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<TemplateItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Individual item in a template save request.
/// </summary>
public class TemplateItemRequest
{
    public Guid ComponentId { get; set; }
    public decimal Quantity { get; set; }
}

/// <summary>
/// Request DTO for applying a template to a product.
/// </summary>
public class ApplyTemplateRequest
{
    public Guid TemplateId { get; set; }
}

/// <summary>
/// Result of applying a template with merge info.
/// </summary>
public class ApplyTemplateResultDto
{
    public List<ComponentAssignmentDto> Assignments { get; set; } = new();
    public List<string> AddedComponents { get; set; } = new();
    public List<string> SkippedComponents { get; set; } = new();
}

/// <summary>
/// DTO for margin report row.
/// </summary>
public class ProductMarginDto
{
    public Guid ProductId { get; set; }
    public required string SKU { get; set; }
    public required string ProductName { get; set; }
    public string? CollectionName { get; set; }
    public decimal OfficialPrice { get; set; }
    public decimal TotalCostPrice { get; set; }
    public decimal TotalSalePrice { get; set; }
    public decimal MarginAmount { get; set; }
    public decimal MarginPercent { get; set; }
}

/// <summary>
/// Margin report query parameters.
/// </summary>
public class MarginReportQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public Guid? CollectionId { get; set; }
    public decimal? MaxMarginPercent { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// Margin report response with aggregated totals.
/// </summary>
public class MarginReportDto
{
    public List<ProductMarginDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public decimal SumCostPrice { get; set; }
    public decimal SumSalePrice { get; set; }
    public decimal SumMargin { get; set; }
}

/// <summary>
/// Products without components report query parameters.
/// </summary>
public class ProductsWithoutComponentsQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public Guid? CollectionId { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// DTO for a product without components (report row).
/// </summary>
public class ProductWithoutComponentsDto
{
    public Guid ProductId { get; set; }
    public required string SKU { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public string? CollectionName { get; set; }
}
