namespace JoiabagurPV.Application.DTOs.Sales;

public class SalesReportFilterRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? PointOfSaleId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? Search { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public bool? HasPhoto { get; set; }
    public bool? PriceWasOverridden { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
