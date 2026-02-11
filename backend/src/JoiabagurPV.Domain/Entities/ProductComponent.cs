namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a component in the master table (materials, labor, etc.) that can be assigned to products.
/// </summary>
public class ProductComponent : BaseEntity
{
    /// <summary>
    /// Description of the component (e.g., "Oro 18k", "Hora trabajo").
    /// Must be unique and max 35 characters.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Optional default cost price from the master table. Decimal(18,4) precision.
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Optional default sale price from the master table. Decimal(18,4) precision.
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// Whether the component is active. Inactive components cannot be assigned to new products
    /// but existing assignments are preserved.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for product assignments using this component.
    /// </summary>
    public ICollection<ProductComponentAssignment> Assignments { get; set; } = new List<ProductComponentAssignment>();

    /// <summary>
    /// Navigation property for template items using this component.
    /// </summary>
    public ICollection<ComponentTemplateItem> TemplateItems { get; set; } = new List<ComponentTemplateItem>();
}
