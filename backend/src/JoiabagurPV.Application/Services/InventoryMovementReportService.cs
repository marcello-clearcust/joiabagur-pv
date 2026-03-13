using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Application.Services;

public class InventoryMovementReportService : IInventoryMovementReportService
{
    private readonly IInventoryMovementRepository _movementRepository;
    private const int ExportLimit = 50_000;

    public InventoryMovementReportService(IInventoryMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<InventoryMovementReportResponse> GetReportAsync(InventoryMovementReportFilterRequest request)
    {
        var query = _movementRepository.GetMovementSummaryByProduct(
            request.StartDate!.Value,
            request.EndDate!.Value,
            request.PointOfSaleId);

        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new InventoryMovementReportResponse
        {
            Items = items.Select(p => new InventoryMovementSummaryRow
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductSku = p.ProductSku,
                Additions = p.Additions,
                Subtractions = p.Subtractions,
                Difference = p.Difference
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<(MemoryStream Stream, int TotalCount)> ExportReportAsync(InventoryMovementReportFilterRequest request)
    {
        var query = _movementRepository.GetMovementSummaryByProduct(
            request.StartDate!.Value,
            request.EndDate!.Value,
            request.PointOfSaleId);

        var totalCount = await query.CountAsync();

        if (totalCount > ExportLimit)
        {
            throw new InvalidOperationException($"EXPORT_LIMIT_EXCEEDED:{totalCount}");
        }

        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var items = await query.Take(ExportLimit).ToListAsync();

        var stream = GenerateExcel(items);
        return (stream, totalCount);
    }

    private static IQueryable<MovementSummaryProjection> ApplySorting(
        IQueryable<MovementSummaryProjection> query,
        string? sortBy,
        string? sortDirection)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "additions" => desc ? query.OrderByDescending(r => r.Additions) : query.OrderBy(r => r.Additions),
            "subtractions" => desc ? query.OrderByDescending(r => r.Subtractions) : query.OrderBy(r => r.Subtractions),
            "difference" => desc ? query.OrderByDescending(r => r.Difference) : query.OrderBy(r => r.Difference),
            _ => desc ? query.OrderByDescending(r => r.ProductName) : query.OrderBy(r => r.ProductName),
        };
    }

    private static MemoryStream GenerateExcel(List<MovementSummaryProjection> items)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Resumen movimientos");

        var headers = new[] { "Producto", "SKU", "Adiciones", "Sustracciones", "Diferencia" };
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        for (var i = 0; i < items.Count; i++)
        {
            var row = i + 2;
            var item = items[i];
            ws.Cell(row, 1).Value = item.ProductName;
            ws.Cell(row, 2).Value = item.ProductSku;
            ws.Cell(row, 3).Value = item.Additions;
            ws.Cell(row, 4).Value = item.Subtractions;
            ws.Cell(row, 5).Value = item.Difference;
        }

        ws.Columns(3, 5).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }
}
