using FluentValidation;
using JoiabagurPV.Application.DTOs.Users;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for update user requests.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido")
            .MaximumLength(256).WithMessage("El email no puede exceder 256 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido")
            .Must(role => role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) || 
                          role.Equals("Operator", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El rol debe ser 'Administrator' u 'Operator'");
    }
}
