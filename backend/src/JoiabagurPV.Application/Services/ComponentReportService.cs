using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for component-related reports (margins and products without components).
/// </summary>
public class ComponentReportService : IComponentReportService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductComponentAssignmentRepository _assignmentRepository;
    private readonly ILogger<ComponentReportService> _logger;

    public ComponentReportService(
        IProductRepository productRepository,
        IProductComponentAssignmentRepository assignmentRepository,
        ILogger<ComponentReportService> logger)
    {
        _productRepository = productRepository;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<MarginReportDto> GetMarginReportAsync(MarginReportQueryParameters parameters)
    {
        var query = BuildMarginQuery(parameters);
        var allItems = await query.ToListAsync();

        // Calculate aggregated totals from all matching items (before pagination)
        var sumCostPrice = allItems.Sum(i => i.TotalCostPrice);
        var sumSalePrice = allItems.Sum(i => i.TotalSalePrice);
        var sumMargin = allItems.Sum(i => i.MarginAmount);

        var totalCount = allItems.Count;
        var pageSize = Math.Min(parameters.PageSize, 50);
        var page = Math.Max(parameters.Page, 1);

        var pagedItems = allItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new MarginReportDto
        {
            Items = pagedItems,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            CurrentPage = page,
            PageSize = pageSize,
            SumCostPrice = sumCostPrice,
            SumSalePrice = sumSalePrice,
            SumMargin = sumMargin
        };
    }

    /// <inheritdoc/>
    public async Task<List<ProductMarginDto>> GetMarginReportForExportAsync(MarginReportQueryParameters parameters)
    {
        var query = BuildMarginQuery(parameters);
        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PaginatedResultDto<ProductWithoutComponentsDto>> GetProductsWithoutComponentsAsync(
        ProductsWithoutComponentsQueryParameters parameters)
    {
        // Get all products, then filter out those with assignments
        var productsQuery = _productRepository.GetAll()
            .Where(p => p.IsActive);

        // Left join to check no assignments exist
        var query = productsQuery
            .Where(p => !_assignmentRepository.GetAll().Any(a => a.ProductId == p.Id));

        // Apply filters
        if (parameters.CollectionId.HasValue)
        {
            query = query.Where(p => p.CollectionId == parameters.CollectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToUpperInvariant();
            query = query.Where(p =>
                p.SKU.ToUpper().Contains(search) ||
                p.Name.ToUpper().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Min(parameters.PageSize, 50);
        var page = Math.Max(parameters.Page, 1);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductWithoutComponentsDto
            {
                ProductId = p.Id,
                SKU = p.SKU,
                ProductName = p.Name,
                Price = p.Price,
                CollectionName = p.Collection != null ? p.Collection.Name : null
            })
            .ToListAsync();

        return PaginatedResultDto<ProductWithoutComponentsDto>.Create(items, totalCount, page, pageSize);
    }

    private IQueryable<ProductMarginDto> BuildMarginQuery(MarginReportQueryParameters parameters)
    {
        // Products with at least one component assignment
        var query = _productRepository.GetAll()
            .Where(p => p.IsActive)
            .Where(p => _assignmentRepository.GetAll().Any(a => a.ProductId == p.Id))
            .Select(p => new ProductMarginDto
            {
                ProductId = p.Id,
                SKU = p.SKU,
                ProductName = p.Name,
                CollectionName = p.Collection != null ? p.Collection.Name : null,
                OfficialPrice = p.Price,
                TotalCostPrice = _assignmentRepository.GetAll()
                    .Where(a => a.ProductId == p.Id)
                    .Sum(a => a.CostPrice * a.Quantity),
                TotalSalePrice = _assignmentRepository.GetAll()
                    .Where(a => a.ProductId == p.Id)
                    .Sum(a => a.SalePrice * a.Quantity),
                MarginAmount = _assignmentRepository.GetAll()
                    .Where(a => a.ProductId == p.Id)
                    .Sum(a => a.SalePrice * a.Quantity) -
                    _assignmentRepository.GetAll()
                    .Where(a => a.ProductId == p.Id)
                    .Sum(a => a.CostPrice * a.Quantity),
                MarginPercent = _assignmentRepository.GetAll()
                    .Where(a => a.ProductId == p.Id)
                    .Sum(a => a.SalePrice * a.Quantity) > 0
                    ? (_assignmentRepository.GetAll()
                        .Where(a => a.ProductId == p.Id)
                        .Sum(a => a.SalePrice * a.Quantity) -
                        _assignmentRepository.GetAll()
                        .Where(a => a.ProductId == p.Id)
                        .Sum(a => a.CostPrice * a.Quantity)) /
                        _assignmentRepository.GetAll()
                        .Where(a => a.ProductId == p.Id)
                        .Sum(a => a.SalePrice * a.Quantity) * 100
                    : 0
            });

        // Apply filters
        if (parameters.CollectionId.HasValue)
        {
            query = query.Where(m => _productRepository.GetAll()
                .Any(p => p.Id == m.ProductId && p.CollectionId == parameters.CollectionId.Value));
        }

        if (parameters.MaxMarginPercent.HasValue)
        {
            query = query.Where(m => m.MarginPercent < parameters.MaxMarginPercent.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToUpperInvariant();
            query = query.Where(m =>
                m.SKU.ToUpper().Contains(search) ||
                m.ProductName.ToUpper().Contains(search));
        }

        return query.OrderBy(m => m.ProductName);
    }
}
