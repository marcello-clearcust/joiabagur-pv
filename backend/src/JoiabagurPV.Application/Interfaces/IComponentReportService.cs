using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for component-related reports.
/// </summary>
public interface IComponentReportService
{
    /// <summary>
    /// Gets the margin report for products with component assignments.
    /// </summary>
    Task<MarginReportDto> GetMarginReportAsync(MarginReportQueryParameters parameters);

    /// <summary>
    /// Gets a paginated list of products without any component assignments.
    /// </summary>
    Task<PaginatedResultDto<ProductWithoutComponentsDto>> GetProductsWithoutComponentsAsync(
        ProductsWithoutComponentsQueryParameters parameters);

    /// <summary>
    /// Exports margin report data as raw DTOs (for Excel generation).
    /// </summary>
    Task<List<ProductMarginDto>> GetMarginReportForExportAsync(MarginReportQueryParameters parameters);
}
