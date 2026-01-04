using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for PointOfSale entity operations.
/// </summary>
public interface IPointOfSaleRepository : IRepository<PointOfSale>
{
    /// <summary>
    /// Gets a point of sale by its unique code.
    /// </summary>
    /// <param name="code">The unique code to search for.</param>
    /// <returns>The point of sale if found, null otherwise.</returns>
    Task<PointOfSale?> GetByCodeAsync(string code);

    /// <summary>
    /// Checks if a code is already in use.
    /// </summary>
    /// <param name="code">The code to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <returns>True if code exists, false otherwise.</returns>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);

    /// <summary>
    /// Gets all points of sale with optional filtering.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive points of sale.</param>
    /// <returns>List of points of sale.</returns>
    Task<List<PointOfSale>> GetAllAsync(bool includeInactive = true);

    /// <summary>
    /// Gets points of sale assigned to a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="includeInactive">Whether to include inactive assignments.</param>
    /// <returns>List of points of sale assigned to the user.</returns>
    Task<List<PointOfSale>> GetByUserAsync(Guid userId, bool includeInactive = false);

    /// <summary>
    /// Gets a point of sale with its operator assignments.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The point of sale with assignments, or null if not found.</returns>
    Task<PointOfSale?> GetWithAssignmentsAsync(Guid pointOfSaleId);

    /// <summary>
    /// Checks if a point of sale has active assignments.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>True if has active assignments, false otherwise.</returns>
    Task<bool> HasActiveAssignmentsAsync(Guid pointOfSaleId);
}
