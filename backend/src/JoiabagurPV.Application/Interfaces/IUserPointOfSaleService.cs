using JoiabagurPV.Application.DTOs.Users;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for user point of sale assignment operations.
/// </summary>
public interface IUserPointOfSaleService
{
    /// <summary>
    /// Gets all assignments for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of assignment DTOs.</returns>
    Task<List<UserPointOfSaleDto>> GetUserAssignmentsAsync(Guid userId);

    /// <summary>
    /// Gets all operators assigned to a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>List of user assignments.</returns>
    Task<List<UserPointOfSaleDto>> GetByPointOfSaleAsync(Guid pointOfSaleId);

    /// <summary>
    /// Assigns a user to a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The assignment DTO.</returns>
    Task<UserPointOfSaleDto> AssignAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Unassigns a user from a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    Task UnassignAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Checks if a user has access to a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>True if user has access.</returns>
    Task<bool> HasAccessAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Gets the point of sale IDs assigned to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of point of sale IDs.</returns>
    Task<List<Guid>> GetAssignedPointOfSaleIdsAsync(Guid userId);
}
