namespace JoiabagurPV.Application.DTOs.Sales;

/// <summary>
/// Request DTO for filtering sales history.
/// </summary>
public class SalesHistoryFilterRequest
{
    /// <summary>
    /// Optional start date filter.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date filter.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Optional point of sale filter.
    /// </summary>
    public Guid? PointOfSaleId { get; set; }

    /// <summary>
    /// Optional product filter.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Optional user filter.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Optional payment method filter.
    /// </summary>
    public Guid? PaymentMethodId { get; set; }

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (max 50).
    /// </summary>
    public int PageSize { get; set; } = 20;
}
