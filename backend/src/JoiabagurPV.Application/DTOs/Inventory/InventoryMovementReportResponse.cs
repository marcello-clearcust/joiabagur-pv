namespace JoiabagurPV.Application.DTOs.Inventory;

public class InventoryMovementReportResponse
{
    public List<InventoryMovementSummaryRow> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
