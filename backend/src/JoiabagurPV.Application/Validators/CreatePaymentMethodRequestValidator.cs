using FluentValidation;
using JoiabagurPV.Application.DTOs.PaymentMethods;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for create payment method requests.
/// </summary>
public class CreatePaymentMethodRequestValidator : AbstractValidator<CreatePaymentMethodRequest>
{
    public CreatePaymentMethodRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres")
            .Matches("^[A-Z0-9_-]+$").WithMessage("El código solo puede contener letras mayúsculas, números, guiones y guiones bajos");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
