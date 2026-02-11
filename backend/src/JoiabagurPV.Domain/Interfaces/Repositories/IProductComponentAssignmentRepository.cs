using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ProductComponentAssignment entity operations.
/// </summary>
public interface IProductComponentAssignmentRepository : IRepository<ProductComponentAssignment>
{
    /// <summary>
    /// Gets all component assignments for a product, ordered by DisplayOrder.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>List of assignments with component details loaded.</returns>
    Task<List<ProductComponentAssignment>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Removes all component assignments for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    Task RemoveAllByProductIdAsync(Guid productId);

    /// <summary>
    /// Adds multiple assignments in a batch.
    /// </summary>
    /// <param name="assignments">The assignments to add.</param>
    Task AddRangeAsync(IEnumerable<ProductComponentAssignment> assignments);

    /// <summary>
    /// Checks if a product has any component assignments.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>True if the product has at least one assignment.</returns>
    Task<bool> HasAssignmentsAsync(Guid productId);
}
