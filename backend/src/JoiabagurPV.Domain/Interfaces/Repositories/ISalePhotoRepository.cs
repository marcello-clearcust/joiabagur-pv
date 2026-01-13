using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for SalePhoto entity operations.
/// </summary>
public interface ISalePhotoRepository : IRepository<SalePhoto>
{
    /// <summary>
    /// Gets the photo for a specific sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>The sale photo if exists, null otherwise.</returns>
    Task<SalePhoto?> GetBySaleIdAsync(Guid saleId);

    /// <summary>
    /// Checks if a sale has a photo attached.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>True if photo exists, false otherwise.</returns>
    Task<bool> ExistsBySaleIdAsync(Guid saleId);

    /// <summary>
    /// Deletes the photo for a specific sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteBySaleIdAsync(Guid saleId);
}
