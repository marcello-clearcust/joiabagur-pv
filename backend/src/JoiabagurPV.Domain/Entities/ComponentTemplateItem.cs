namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a component entry within a template, defining component and quantity (no prices).
/// </summary>
public class ComponentTemplateItem : BaseEntity
{
    /// <summary>
    /// Foreign key to the template.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Navigation property for the associated template.
    /// </summary>
    public ComponentTemplate? Template { get; set; }

    /// <summary>
    /// Foreign key to the component from the master table.
    /// </summary>
    public Guid ComponentId { get; set; }

    /// <summary>
    /// Navigation property for the associated component.
    /// </summary>
    public ProductComponent? Component { get; set; }

    /// <summary>
    /// Quantity of the component in the template. Must be > 0. Decimal(18,4) precision.
    /// </summary>
    public decimal Quantity { get; set; }
}
