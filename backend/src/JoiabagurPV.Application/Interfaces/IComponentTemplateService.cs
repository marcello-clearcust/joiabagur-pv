using JoiabagurPV.Application.DTOs.Components;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for component template management.
/// </summary>
public interface IComponentTemplateService
{
    /// <summary>
    /// Gets all templates with their items.
    /// </summary>
    Task<List<ComponentTemplateDto>> GetAllAsync();

    /// <summary>
    /// Gets a template by ID with items.
    /// </summary>
    Task<ComponentTemplateDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new template.
    /// </summary>
    Task<ComponentTemplateDto> CreateAsync(SaveComponentTemplateRequest request);

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    Task<ComponentTemplateDto> UpdateAsync(Guid id, SaveComponentTemplateRequest request);

    /// <summary>
    /// Deletes a template.
    /// </summary>
    Task DeleteAsync(Guid id);
}
