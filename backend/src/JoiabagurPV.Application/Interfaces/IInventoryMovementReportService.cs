using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Interfaces;

public interface IInventoryMovementReportService
{
    Task<InventoryMovementReportResponse> GetReportAsync(InventoryMovementReportFilterRequest request);
    Task<(MemoryStream Stream, int TotalCount)> ExportReportAsync(InventoryMovementReportFilterRequest request);
}
