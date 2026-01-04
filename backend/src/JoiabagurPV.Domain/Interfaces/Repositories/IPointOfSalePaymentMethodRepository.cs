using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for PointOfSalePaymentMethod entity operations.
/// </summary>
public interface IPointOfSalePaymentMethodRepository : IRepository<PointOfSalePaymentMethod>
{
    /// <summary>
    /// Gets all payment methods assigned to a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="includeInactive">Whether to include inactive assignments.</param>
    /// <returns>List of payment method assignments.</returns>
    Task<List<PointOfSalePaymentMethod>> GetByPointOfSaleAsync(Guid pointOfSaleId, bool includeInactive = false);

    /// <summary>
    /// Gets a specific assignment by point of sale and payment method.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <returns>The assignment if found, null otherwise.</returns>
    Task<PointOfSalePaymentMethod?> GetAssignmentAsync(Guid pointOfSaleId, Guid paymentMethodId);

    /// <summary>
    /// Checks if a payment method is assigned to a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <returns>True if assigned, false otherwise.</returns>
    Task<bool> IsAssignedAsync(Guid pointOfSaleId, Guid paymentMethodId);

    /// <summary>
    /// Checks if a payment method is active for a specific point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <returns>True if the payment method is active for the point of sale.</returns>
    Task<bool> IsActiveForPointOfSaleAsync(Guid pointOfSaleId, Guid paymentMethodId);

    /// <summary>
    /// Gets the count of active assignments for a payment method.
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <returns>The number of active assignments.</returns>
    Task<int> GetActiveAssignmentCountAsync(Guid paymentMethodId);
}
