using FluentValidation;
using JoiabagurPV.Application.DTOs.PointOfSales;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for update point of sale requests.
/// </summary>
public class UpdatePointOfSaleRequestValidator : AbstractValidator<UpdatePointOfSaleRequest>
{
    public UpdatePointOfSaleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Address)
            .MaximumLength(256).WithMessage("La dirección no puede exceder 256 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("El teléfono tiene un formato inválido")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido")
            .MaximumLength(256).WithMessage("El email no puede exceder 256 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
