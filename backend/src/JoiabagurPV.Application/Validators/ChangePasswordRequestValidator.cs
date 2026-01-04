using FluentValidation;
using JoiabagurPV.Application.DTOs.Users;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for change password requests.
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");
    }
}
