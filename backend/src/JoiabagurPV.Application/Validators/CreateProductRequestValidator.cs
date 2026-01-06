using FluentValidation;
using JoiabagurPV.Application.DTOs.Products;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for creating a product.
/// </summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("El SKU es requerido")
            .MaximumLength(50).WithMessage("El SKU no puede exceder 50 caracteres");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor que 0");
    }
}


