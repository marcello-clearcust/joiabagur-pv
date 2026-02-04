namespace JoiabagurPV.Application.DTOs.Returns;

/// <summary>
/// Response DTO for eligible sales query.
/// </summary>
public class EligibleSalesResponse
{
    /// <summary>
    /// List of sales eligible for return.
    /// </summary>
    public List<EligibleSaleDto> EligibleSales { get; set; } = new();

    /// <summary>
    /// Total quantity available for return across all eligible sales.
    /// </summary>
    public int TotalAvailableForReturn { get; set; }
}

/// <summary>
/// DTO for an eligible sale.
/// </summary>
public class EligibleSaleDto
{
    /// <summary>
    /// Sale ID.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// When the sale occurred.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Original quantity sold.
    /// </summary>
    public int OriginalQuantity { get; set; }

    /// <summary>
    /// Quantity already returned from this sale.
    /// </summary>
    public int ReturnedQuantity { get; set; }

    /// <summary>
    /// Quantity still available for return.
    /// </summary>
    public int AvailableForReturn { get; set; }

    /// <summary>
    /// Unit price from the sale (for return value calculation).
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Payment method used in the sale.
    /// </summary>
    public string PaymentMethodName { get; set; } = string.Empty;

    /// <summary>
    /// Days remaining until this sale is no longer eligible (30-day window).
    /// </summary>
    public int DaysRemaining { get; set; }
}
