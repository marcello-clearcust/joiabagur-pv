using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for product component master table operations.
/// </summary>
public class ProductComponentService : IProductComponentService
{
    private readonly IProductComponentRepository _componentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductComponentService> _logger;

    public ProductComponentService(
        IProductComponentRepository componentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProductComponentService> logger)
    {
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResultDto<ComponentResponseDto>> GetComponentsAsync(ComponentQueryParameters parameters)
    {
        var query = _componentRepository.GetAll();

        // Filter by active status
        if (parameters.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == parameters.IsActive.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(parameters.Search) && parameters.Search.Length >= 2)
        {
            var searchTerm = parameters.Search.Trim().ToUpperInvariant();
            query = query.Where(c => c.Description.ToUpper().Contains(searchTerm));
        }

        var totalCount = query.Count();
        var pageSize = Math.Min(parameters.PageSize, 50);
        var page = Math.Max(parameters.Page, 1);

        var items = query
            .OrderBy(c => c.Description)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToList();

        return PaginatedResultDto<ComponentResponseDto>.Create(items, totalCount, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<ComponentResponseDto?> GetByIdAsync(Guid id)
    {
        var component = await _componentRepository.GetByIdAsync(id);
        return component == null ? null : MapToDto(component);
    }

    /// <inheritdoc/>
    public async Task<ComponentResponseDto> CreateAsync(CreateComponentRequest request)
    {
        // Validate description
        var description = (request.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descripción es obligatoria.");
        if (description.Length > 35)
            throw new ArgumentException("La descripción no puede superar 35 caracteres.");

        // Validate uniqueness
        if (await _componentRepository.DescriptionExistsAsync(description))
            throw new ArgumentException("La descripción ya existe.");

        // Validate prices
        if (request.CostPrice.HasValue && request.CostPrice.Value < 0)
            throw new ArgumentException("El precio de coste debe ser >= 0.");
        if (request.SalePrice.HasValue && request.SalePrice.Value < 0)
            throw new ArgumentException("El precio de venta debe ser >= 0.");

        var component = new ProductComponent
        {
            Description = description,
            CostPrice = request.CostPrice,
            SalePrice = request.SalePrice,
            IsActive = true
        };

        await _componentRepository.AddAsync(component);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created component {ComponentId} with description '{Description}'", component.Id, description);

        return MapToDto(component);
    }

    /// <inheritdoc/>
    public async Task<ComponentResponseDto> UpdateAsync(Guid id, UpdateComponentRequest request)
    {
        var component = await _componentRepository.GetByIdAsync(id);
        if (component == null)
            throw new KeyNotFoundException($"Componente con ID {id} no encontrado.");

        // Validate description
        var description = (request.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descripción es obligatoria.");
        if (description.Length > 35)
            throw new ArgumentException("La descripción no puede superar 35 caracteres.");

        // Validate uniqueness (exclude self)
        if (await _componentRepository.DescriptionExistsAsync(description, id))
            throw new ArgumentException("La descripción ya existe.");

        // Validate prices
        if (request.CostPrice.HasValue && request.CostPrice.Value < 0)
            throw new ArgumentException("El precio de coste debe ser >= 0.");
        if (request.SalePrice.HasValue && request.SalePrice.Value < 0)
            throw new ArgumentException("El precio de venta debe ser >= 0.");

        component.Description = description;
        component.CostPrice = request.CostPrice;
        component.SalePrice = request.SalePrice;
        component.IsActive = request.IsActive;

        await _componentRepository.UpdateAsync(component);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated component {ComponentId}", id);

        return MapToDto(component);
    }

    /// <inheritdoc/>
    public async Task<List<ComponentResponseDto>> SearchAsync(string query, int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return new List<ComponentResponseDto>();

        var components = await _componentRepository.SearchActiveAsync(query.Trim(), maxResults);
        return components.Select(MapToDto).ToList();
    }

    private static ComponentResponseDto MapToDto(ProductComponent component)
    {
        return new ComponentResponseDto
        {
            Id = component.Id,
            Description = component.Description,
            CostPrice = component.CostPrice,
            SalePrice = component.SalePrice,
            IsActive = component.IsActive,
            CreatedAt = component.CreatedAt,
            UpdatedAt = component.UpdatedAt
        };
    }
}
