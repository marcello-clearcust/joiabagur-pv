namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents the assignment of a component to a product with quantity and override prices.
/// </summary>
public class ProductComponentAssignment : BaseEntity
{
    /// <summary>
    /// Foreign key to the product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property for the associated product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Foreign key to the component from the master table.
    /// </summary>
    public Guid ComponentId { get; set; }

    /// <summary>
    /// Navigation property for the associated component.
    /// </summary>
    public ProductComponent? Component { get; set; }

    /// <summary>
    /// Quantity of the component used in the product. Must be > 0. Decimal(18,4) precision.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Override cost price for this specific assignment. Decimal(18,4) precision.
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Override sale price for this specific assignment. Decimal(18,4) precision.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Display order for drag-and-drop reordering.
    /// </summary>
    public int DisplayOrder { get; set; }
}
