using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ReturnSale entity operations.
/// </summary>
public interface IReturnSaleRepository : IRepository<ReturnSale>
{
    /// <summary>
    /// Gets the total quantity already returned for a specific sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>Total quantity returned from this sale.</returns>
    Task<int> GetReturnedQuantityForSaleAsync(Guid saleId);

    /// <summary>
    /// Gets all return associations for a specific sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>List of return-sale associations.</returns>
    Task<List<ReturnSale>> GetBySaleIdAsync(Guid saleId);

    /// <summary>
    /// Gets all return associations for a specific return.
    /// </summary>
    /// <param name="returnId">The return ID.</param>
    /// <returns>List of return-sale associations with sale details.</returns>
    Task<List<ReturnSale>> GetByReturnIdWithSaleDetailsAsync(Guid returnId);
}
