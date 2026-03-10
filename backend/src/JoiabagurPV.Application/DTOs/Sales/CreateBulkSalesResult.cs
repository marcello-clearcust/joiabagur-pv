namespace JoiabagurPV.Application.DTOs.Sales;

public class CreateBulkSalesResult
{
    public bool Success { get; set; }
    public Guid? BulkOperationId { get; set; }
    public List<SaleDto> Sales { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public List<BulkSaleLineWarning> Warnings { get; set; } = new();
}

public class BulkSaleLineWarning
{
    public int LineIndex { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsLowStock { get; set; }
    public int RemainingStock { get; set; }
}
