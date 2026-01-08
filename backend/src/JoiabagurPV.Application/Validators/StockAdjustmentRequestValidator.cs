using FluentValidation;
using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for StockAdjustmentRequest.
/// </summary>
public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("El ID del punto de venta es requerido.");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0)
            .WithMessage("El cambio de cantidad no puede ser cero.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("El motivo del ajuste es requerido.")
            .MaximumLength(500)
            .WithMessage("El motivo del ajuste no puede exceder 500 caracteres.");
    }
}

