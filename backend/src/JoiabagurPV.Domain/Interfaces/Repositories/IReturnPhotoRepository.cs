using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ReturnPhoto entity operations.
/// </summary>
public interface IReturnPhotoRepository : IRepository<ReturnPhoto>
{
    /// <summary>
    /// Gets the photo for a specific return.
    /// </summary>
    /// <param name="returnId">The return ID.</param>
    /// <returns>The photo if it exists, null otherwise.</returns>
    Task<ReturnPhoto?> GetByReturnIdAsync(Guid returnId);
}
