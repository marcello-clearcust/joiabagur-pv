using FluentValidation;
using JoiabagurPV.Application.DTOs.Returns;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for CreateReturnRequest.
/// </summary>
public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequest>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("El ID del punto de venta es requerido.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor que cero.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("La categoría de devolución es inválida.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(x => x.SaleAssociations)
            .NotEmpty()
            .WithMessage("Debe seleccionar al menos una venta para asociar.");

        RuleForEach(x => x.SaleAssociations)
            .ChildRules(sale =>
            {
                sale.RuleFor(s => s.SaleId)
                    .NotEmpty()
                    .WithMessage("El ID de la venta es requerido.");

                sale.RuleFor(s => s.Quantity)
                    .GreaterThan(0)
                    .WithMessage("La cantidad por venta debe ser mayor que cero.");
            });

        RuleFor(x => x.PhotoBase64)
            .Must(BeValidBase64)
            .When(x => !string.IsNullOrEmpty(x.PhotoBase64))
            .WithMessage("La foto debe ser un dato Base64 válido.");
    }

    private bool BeValidBase64(string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            return true;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
