using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for component template management.
/// </summary>
public class ComponentTemplateService : IComponentTemplateService
{
    private readonly IComponentTemplateRepository _templateRepository;
    private readonly IProductComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ComponentTemplateService> _logger;

    public ComponentTemplateService(
        IComponentTemplateRepository templateRepository,
        IProductComponentRepository componentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ComponentTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<ComponentTemplateDto>> GetAllAsync()
    {
        var templates = await _templateRepository.GetAllWithItemsAsync();
        return templates.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<ComponentTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _templateRepository.GetWithItemsAsync(id);
        return template == null ? null : MapToDto(template);
    }

    /// <inheritdoc/>
    public async Task<ComponentTemplateDto> CreateAsync(SaveComponentTemplateRequest request)
    {
        ValidateRequest(request);

        var template = new ComponentTemplate
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Items = request.Items.Select(i => new ComponentTemplateItem
            {
                ComponentId = i.ComponentId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _templateRepository.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created template {TemplateId} '{TemplateName}' with {ItemCount} items",
            template.Id, template.Name, template.Items.Count);

        // Reload with navigation properties
        var created = await _templateRepository.GetWithItemsAsync(template.Id);
        return MapToDto(created!);
    }

    /// <inheritdoc/>
    public async Task<ComponentTemplateDto> UpdateAsync(Guid id, SaveComponentTemplateRequest request)
    {
        var template = await _templateRepository.GetWithItemsAsync(id);
        if (template == null)
            throw new KeyNotFoundException($"Plantilla con ID {id} no encontrada.");

        ValidateRequest(request);

        template.Name = request.Name.Trim();
        template.Description = request.Description?.Trim();

        // Replace items
        template.Items.Clear();
        foreach (var item in request.Items)
        {
            template.Items.Add(new ComponentTemplateItem
            {
                TemplateId = id,
                ComponentId = item.ComponentId,
                Quantity = item.Quantity
            });
        }

        await _templateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated template {TemplateId}", id);

        var updated = await _templateRepository.GetWithItemsAsync(id);
        return MapToDto(updated!);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var exists = await _templateRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"Plantilla con ID {id} no encontrada.");

        await _templateRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted template {TemplateId}", id);
    }

    private void ValidateRequest(SaveComponentTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("El nombre de la plantilla es obligatorio.");

        // Validate no duplicate components
        var componentIds = request.Items.Select(i => i.ComponentId).ToList();
        if (componentIds.Count != componentIds.Distinct().Count())
            throw new ArgumentException("No se puede repetir el mismo componente en una plantilla.");

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.");
        }
    }

    private static ComponentTemplateDto MapToDto(ComponentTemplate template)
    {
        return new ComponentTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Items = template.Items.Select(i => new ComponentTemplateItemDto
            {
                ComponentId = i.ComponentId,
                ComponentDescription = i.Component?.Description ?? string.Empty,
                Quantity = i.Quantity
            }).ToList(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
