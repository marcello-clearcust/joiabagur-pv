namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a reusable template of components with quantities for quick product setup.
/// </summary>
public class ComponentTemplate : BaseEntity
{
    /// <summary>
    /// Name of the template (e.g., "Anillo oro").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the template.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property for the template items (component + quantity pairs).
    /// </summary>
    public ICollection<ComponentTemplateItem> Items { get; set; } = new List<ComponentTemplateItem>();
}
