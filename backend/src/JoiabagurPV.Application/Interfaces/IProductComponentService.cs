using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for product component master table operations.
/// </summary>
public interface IProductComponentService
{
    /// <summary>
    /// Gets a paginated list of components with optional filters.
    /// </summary>
    Task<PaginatedResultDto<ComponentResponseDto>> GetComponentsAsync(ComponentQueryParameters parameters);

    /// <summary>
    /// Gets a component by ID.
    /// </summary>
    Task<ComponentResponseDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new component.
    /// </summary>
    Task<ComponentResponseDto> CreateAsync(CreateComponentRequest request);

    /// <summary>
    /// Updates an existing component.
    /// </summary>
    Task<ComponentResponseDto> UpdateAsync(Guid id, UpdateComponentRequest request);

    /// <summary>
    /// Searches active components by description for autocomplete.
    /// </summary>
    Task<List<ComponentResponseDto>> SearchAsync(string query, int maxResults = 20);
}
