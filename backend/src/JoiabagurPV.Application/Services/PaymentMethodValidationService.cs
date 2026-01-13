using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for validating payment method availability at points of sale.
/// </summary>
public class PaymentMethodValidationService : IPaymentMethodValidationService
{
    private readonly IPointOfSalePaymentMethodRepository _posPaymentMethodRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentMethodValidationService(
        IPointOfSalePaymentMethodRepository posPaymentMethodRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        _posPaymentMethodRepository = posPaymentMethodRepository ?? throw new ArgumentNullException(nameof(posPaymentMethodRepository));
        _paymentMethodRepository = paymentMethodRepository ?? throw new ArgumentNullException(nameof(paymentMethodRepository));
    }

    /// <inheritdoc/>
    public async Task<bool> IsPaymentMethodAvailableAsync(Guid paymentMethodId, Guid pointOfSaleId)
    {
        // First check if payment method exists and is active
        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null || !paymentMethod.IsActive)
        {
            return false;
        }

        // Check if payment method is assigned to the point of sale and is active
        var assignment = await _posPaymentMethodRepository
            .GetAll()
            .FirstOrDefaultAsync(pm => 
                pm.PaymentMethodId == paymentMethodId && 
                pm.PointOfSaleId == pointOfSaleId &&
                pm.IsActive);

        return assignment != null;
    }

    /// <inheritdoc/>
    public async Task ValidatePaymentMethodAsync(Guid paymentMethodId, Guid pointOfSaleId)
    {
        // First check if payment method exists and is active
        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            throw new InvalidOperationException($"Payment method with ID {paymentMethodId} not found.");
        }

        if (!paymentMethod.IsActive)
        {
            throw new InvalidOperationException($"Payment method '{paymentMethod.Name}' is not active.");
        }

        // Check if payment method is assigned to the point of sale and is active
        var assignment = await _posPaymentMethodRepository
            .GetAll()
            .FirstOrDefaultAsync(pm => 
                pm.PaymentMethodId == paymentMethodId && 
                pm.PointOfSaleId == pointOfSaleId);

        if (assignment == null)
        {
            throw new InvalidOperationException(
                $"Payment method '{paymentMethod.Name}' is not assigned to this point of sale.");
        }

        if (!assignment.IsActive)
        {
            throw new InvalidOperationException(
                $"Payment method '{paymentMethod.Name}' is not active for this point of sale.");
        }
    }
}
