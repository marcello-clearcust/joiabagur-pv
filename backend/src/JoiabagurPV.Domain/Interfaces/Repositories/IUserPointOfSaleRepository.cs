using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserPointOfSale entity operations.
/// </summary>
public interface IUserPointOfSaleRepository : IRepository<UserPointOfSale>
{
    /// <summary>
    /// Gets all assignments for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="activeOnly">If true, only returns active assignments.</param>
    /// <returns>List of assignments.</returns>
    Task<List<UserPointOfSale>> GetByUserIdAsync(Guid userId, bool activeOnly = false);

    /// <summary>
    /// Gets all assignments for a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="activeOnly">If true, only returns active assignments.</param>
    /// <returns>List of assignments.</returns>
    Task<List<UserPointOfSale>> GetByPointOfSaleAsync(Guid pointOfSaleId, bool activeOnly = false);

    /// <summary>
    /// Gets a specific assignment.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The assignment if found, null otherwise.</returns>
    Task<UserPointOfSale?> GetAssignmentAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Gets the active assignment between a user and point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The active assignment if found, null otherwise.</returns>
    Task<UserPointOfSale?> GetActiveAssignmentAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Checks if user has access to a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>True if user has active assignment, false otherwise.</returns>
    Task<bool> HasAccessAsync(Guid userId, Guid pointOfSaleId);

    /// <summary>
    /// Gets all point of sale IDs assigned to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of point of sale IDs.</returns>
    Task<List<Guid>> GetAssignedPointOfSaleIdsAsync(Guid userId);

    /// <summary>
    /// Counts active assignments for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Number of active assignments.</returns>
    Task<int> CountActiveAssignmentsAsync(Guid userId);
}
