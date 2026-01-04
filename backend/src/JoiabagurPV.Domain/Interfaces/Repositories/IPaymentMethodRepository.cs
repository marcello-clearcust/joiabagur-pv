using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for PaymentMethod entity operations.
/// </summary>
public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    /// <summary>
    /// Gets a payment method by its unique code.
    /// </summary>
    /// <param name="code">The unique code to search for.</param>
    /// <returns>The payment method if found, null otherwise.</returns>
    Task<PaymentMethod?> GetByCodeAsync(string code);

    /// <summary>
    /// Checks if a code is already in use.
    /// </summary>
    /// <param name="code">The code to check.</param>
    /// <param name="excludeId">Optional ID to exclude from the check.</param>
    /// <returns>True if code exists, false otherwise.</returns>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);

    /// <summary>
    /// Gets all payment methods with optional filtering.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive payment methods.</param>
    /// <returns>List of payment methods.</returns>
    Task<List<PaymentMethod>> GetAllAsync(bool includeInactive = true);
}
