using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Application.DTOs.Returns;

/// <summary>
/// DTO for return details.
/// </summary>
public class ReturnDto
{
    /// <summary>
    /// Unique identifier of the return.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// Point of sale name.
    /// </summary>
    public string PointOfSaleName { get; set; } = string.Empty;

    /// <summary>
    /// User ID who registered the return.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username who registered the return.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Total quantity returned.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Category of the return.
    /// </summary>
    public ReturnCategory Category { get; set; }

    /// <summary>
    /// Category name for display.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Optional reason for the return.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Total return value (sum of all associated sales' prices).
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Whether the return has a photo attached.
    /// </summary>
    public bool HasPhoto { get; set; }

    /// <summary>
    /// When the return occurred.
    /// </summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>
    /// When the return record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Associated sales with their quantities.
    /// </summary>
    public List<ReturnSaleDto> AssociatedSales { get; set; } = new();
}

/// <summary>
/// DTO for return-sale association details.
/// </summary>
public class ReturnSaleDto
{
    /// <summary>
    /// Sale ID.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Original sale date.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Quantity returned from this sale.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price snapshot from the sale.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Subtotal for this association.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Payment method used in the original sale.
    /// </summary>
    public string PaymentMethodName { get; set; } = string.Empty;
}
