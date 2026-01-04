using JoiabagurPV.Application.DTOs.PointOfSales;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for point of sale management operations.
/// </summary>
public interface IPointOfSaleService
{
    /// <summary>
    /// Gets all points of sale.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive points of sale.</param>
    /// <returns>List of point of sale DTOs.</returns>
    Task<List<PointOfSaleDto>> GetAllAsync(bool includeInactive = true);

    /// <summary>
    /// Gets points of sale accessible to a specific user (for operators).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of point of sale DTOs assigned to the user.</returns>
    Task<List<PointOfSaleDto>> GetByUserAsync(Guid userId);

    /// <summary>
    /// Gets a point of sale by ID.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The point of sale DTO if found.</returns>
    Task<PointOfSaleDto?> GetByIdAsync(Guid pointOfSaleId);

    /// <summary>
    /// Creates a new point of sale.
    /// </summary>
    /// <param name="request">The create point of sale request.</param>
    /// <returns>The created point of sale DTO.</returns>
    Task<PointOfSaleDto> CreateAsync(CreatePointOfSaleRequest request);

    /// <summary>
    /// Updates an existing point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID to update.</param>
    /// <param name="request">The update point of sale request.</param>
    /// <returns>The updated point of sale DTO.</returns>
    Task<PointOfSaleDto> UpdateAsync(Guid pointOfSaleId, UpdatePointOfSaleRequest request);

    /// <summary>
    /// Changes the status (active/inactive) of a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="isActive">The new active status.</param>
    /// <returns>The updated point of sale DTO.</returns>
    Task<PointOfSaleDto> ChangeStatusAsync(Guid pointOfSaleId, bool isActive);

    /// <summary>
    /// Checks if a user has access to a specific point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>True if user has access, false otherwise.</returns>
    Task<bool> UserHasAccessAsync(Guid userId, Guid pointOfSaleId);
}
