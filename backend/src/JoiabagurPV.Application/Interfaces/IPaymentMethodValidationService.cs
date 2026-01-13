namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service for validating payment method availability at points of sale.
/// </summary>
public interface IPaymentMethodValidationService
{
    /// <summary>
    /// Validates that a payment method is available and active for a specific point of sale.
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>True if payment method is available and active, false otherwise.</returns>
    Task<bool> IsPaymentMethodAvailableAsync(Guid paymentMethodId, Guid pointOfSaleId);

    /// <summary>
    /// Validates that a payment method is available and active for a specific point of sale.
    /// Throws an exception if validation fails.
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <exception cref="InvalidOperationException">Thrown when payment method is not available or inactive.</exception>
    Task ValidatePaymentMethodAsync(Guid paymentMethodId, Guid pointOfSaleId);
}
