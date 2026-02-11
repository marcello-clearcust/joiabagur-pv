using ClosedXML.Excel;
using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for component-related reports.
/// All endpoints require Administrator role.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Administrator")]
public class ComponentReportsController : ControllerBase
{
    private readonly IComponentReportService _reportService;
    private readonly ILogger<ComponentReportsController> _logger;

    public ComponentReportsController(
        IComponentReportService reportService,
        ILogger<ComponentReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the margin report for products with component assignments.
    /// </summary>
    [HttpGet("product-margins")]
    public async Task<IActionResult> GetMarginReport([FromQuery] MarginReportQueryParameters parameters)
    {
        var report = await _reportService.GetMarginReportAsync(parameters);
        return Ok(report);
    }

    /// <summary>
    /// Exports the margin report as an Excel file.
    /// </summary>
    [HttpGet("product-margins/export")]
    public async Task<IActionResult> ExportMarginReport([FromQuery] MarginReportQueryParameters parameters)
    {
        var items = await _reportService.GetMarginReportForExportAsync(parameters);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Márgenes por Producto");

        // Headers
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Producto";
        worksheet.Cell(1, 3).Value = "Colección";
        worksheet.Cell(1, 4).Value = "Precio Oficial (€)";
        worksheet.Cell(1, 5).Value = "Coste Total (€)";
        worksheet.Cell(1, 6).Value = "Precio Venta Total (€)";
        worksheet.Cell(1, 7).Value = "Margen (€)";
        worksheet.Cell(1, 8).Value = "Margen (%)";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data rows
        for (var i = 0; i < items.Count; i++)
        {
            var row = i + 2;
            var item = items[i];
            worksheet.Cell(row, 1).Value = item.SKU;
            worksheet.Cell(row, 2).Value = item.ProductName;
            worksheet.Cell(row, 3).Value = item.CollectionName ?? "";
            worksheet.Cell(row, 4).Value = item.OfficialPrice;
            worksheet.Cell(row, 5).Value = item.TotalCostPrice;
            worksheet.Cell(row, 6).Value = item.TotalSalePrice;
            worksheet.Cell(row, 7).Value = item.MarginAmount;
            worksheet.Cell(row, 8).Value = Math.Round(item.MarginPercent, 2);
        }

        // Format number columns
        worksheet.Columns(4, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00\"%\"";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"margenes-productos-{DateTime.UtcNow:yyyy-MM-dd}.xlsx");
    }

    /// <summary>
    /// Gets a paginated list of products without any component assignments.
    /// </summary>
    [HttpGet("products-without-components")]
    public async Task<IActionResult> GetProductsWithoutComponents(
        [FromQuery] ProductsWithoutComponentsQueryParameters parameters)
    {
        var result = await _reportService.GetProductsWithoutComponentsAsync(parameters);
        return Ok(result);
    }
}
