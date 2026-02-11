using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for component assignment operations on products.
/// </summary>
public class ComponentAssignmentService : IComponentAssignmentService
{
    private readonly IProductComponentAssignmentRepository _assignmentRepository;
    private readonly IProductComponentRepository _componentRepository;
    private readonly IProductRepository _productRepository;
    private readonly IComponentTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ComponentAssignmentService> _logger;

    public ComponentAssignmentService(
        IProductComponentAssignmentRepository assignmentRepository,
        IProductComponentRepository componentRepository,
        IProductRepository productRepository,
        IComponentTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<ComponentAssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _componentRepository = componentRepository;
        _productRepository = productRepository;
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<ComponentAssignmentDto>> GetByProductIdAsync(Guid productId)
    {
        var assignments = await _assignmentRepository.GetByProductIdAsync(productId);
        return assignments.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ComponentAssignmentDto>> SaveAssignmentsAsync(Guid productId, SaveComponentAssignmentsRequest request)
    {
        // Validate product exists
        if (!await _productRepository.ExistsAsync(productId))
            throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");

        // Validate no duplicate components
        var componentIds = request.Assignments.Select(a => a.ComponentId).ToList();
        if (componentIds.Count != componentIds.Distinct().Count())
            throw new ArgumentException("No se puede asignar el mismo componente más de una vez.");

        // Validate each assignment
        foreach (var item in request.Assignments)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.");
            if (item.CostPrice < 0)
                throw new ArgumentException("El precio de coste debe ser >= 0.");
            if (item.SalePrice < 0)
                throw new ArgumentException("El precio de venta debe ser >= 0.");

            // Validate component exists
            if (!await _componentRepository.ExistsAsync(item.ComponentId))
                throw new KeyNotFoundException($"Componente con ID {item.ComponentId} no encontrado.");
        }

        // Replace all assignments
        await _assignmentRepository.RemoveAllByProductIdAsync(productId);

        var newAssignments = request.Assignments.Select(item => new ProductComponentAssignment
        {
            ProductId = productId,
            ComponentId = item.ComponentId,
            Quantity = item.Quantity,
            CostPrice = item.CostPrice,
            SalePrice = item.SalePrice,
            DisplayOrder = item.DisplayOrder
        }).ToList();

        if (newAssignments.Any())
        {
            await _assignmentRepository.AddRangeAsync(newAssignments);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Saved {Count} component assignments for product {ProductId}", newAssignments.Count, productId);

        return await GetByProductIdAsync(productId);
    }

    /// <inheritdoc/>
    public async Task<PriceSyncPreviewDto> GetSyncPreviewAsync(Guid productId)
    {
        var assignments = await _assignmentRepository.GetByProductIdAsync(productId);
        var preview = new PriceSyncPreviewDto();

        foreach (var assignment in assignments)
        {
            var component = assignment.Component!;
            var willBeUpdated = component.CostPrice.HasValue || component.SalePrice.HasValue;

            preview.Items.Add(new PriceSyncItemDto
            {
                ComponentId = component.Id,
                ComponentDescription = component.Description,
                CurrentCostPrice = assignment.CostPrice,
                CurrentSalePrice = assignment.SalePrice,
                NewCostPrice = component.CostPrice,
                NewSalePrice = component.SalePrice,
                WillBeUpdated = willBeUpdated
            });
        }

        return preview;
    }

    /// <inheritdoc/>
    public async Task<List<ComponentAssignmentDto>> ApplySyncFromMasterAsync(Guid productId)
    {
        var assignments = await _assignmentRepository.GetByProductIdAsync(productId);

        foreach (var assignment in assignments)
        {
            var component = assignment.Component!;

            if (component.CostPrice.HasValue)
                assignment.CostPrice = component.CostPrice.Value;

            if (component.SalePrice.HasValue)
                assignment.SalePrice = component.SalePrice.Value;

            await _assignmentRepository.UpdateAsync(assignment);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Applied master prices to {Count} assignments for product {ProductId}", assignments.Count, productId);

        return await GetByProductIdAsync(productId);
    }

    /// <inheritdoc/>
    public async Task<ApplyTemplateResultDto> ApplyTemplateAsync(Guid productId, Guid templateId)
    {
        // Validate product exists
        if (!await _productRepository.ExistsAsync(productId))
            throw new KeyNotFoundException($"Producto con ID {productId} no encontrado.");

        var template = await _templateRepository.GetWithItemsAsync(templateId);
        if (template == null)
            throw new KeyNotFoundException($"Plantilla con ID {templateId} no encontrada.");

        var existingAssignments = await _assignmentRepository.GetByProductIdAsync(productId);
        var existingComponentIds = existingAssignments.Select(a => a.ComponentId).ToHashSet();

        var added = new List<string>();
        var skipped = new List<string>();
        var maxDisplayOrder = existingAssignments.Any()
            ? existingAssignments.Max(a => a.DisplayOrder)
            : -1;

        var newAssignments = new List<ProductComponentAssignment>();

        foreach (var templateItem in template.Items)
        {
            var component = templateItem.Component!;

            if (existingComponentIds.Contains(templateItem.ComponentId))
            {
                skipped.Add(component.Description);
                continue;
            }

            maxDisplayOrder++;
            newAssignments.Add(new ProductComponentAssignment
            {
                ProductId = productId,
                ComponentId = templateItem.ComponentId,
                Quantity = templateItem.Quantity,
                CostPrice = component.CostPrice ?? 0,
                SalePrice = component.SalePrice ?? 0,
                DisplayOrder = maxDisplayOrder
            });

            added.Add(component.Description);
        }

        if (newAssignments.Any())
        {
            await _assignmentRepository.AddRangeAsync(newAssignments);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Applied template '{TemplateName}' to product {ProductId}: {Added} added, {Skipped} skipped",
            template.Name, productId, added.Count, skipped.Count);

        var allAssignments = await GetByProductIdAsync(productId);

        return new ApplyTemplateResultDto
        {
            Assignments = allAssignments,
            AddedComponents = added,
            SkippedComponents = skipped
        };
    }

    private static ComponentAssignmentDto MapToDto(ProductComponentAssignment assignment)
    {
        return new ComponentAssignmentDto
        {
            Id = assignment.Id,
            ComponentId = assignment.ComponentId,
            ComponentDescription = assignment.Component?.Description ?? string.Empty,
            Quantity = assignment.Quantity,
            CostPrice = assignment.CostPrice,
            SalePrice = assignment.SalePrice,
            DisplayOrder = assignment.DisplayOrder,
            MasterCostPrice = assignment.Component?.CostPrice,
            MasterSalePrice = assignment.Component?.SalePrice
        };
    }
}
