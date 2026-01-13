using FluentValidation;
using JoiabagurPV.Application.DTOs.Sales;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for CreateSaleRequest.
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("Point of Sale ID is required.");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Payment Method ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.PhotoBase64)
            .Must(BeValidBase64)
            .When(x => !string.IsNullOrEmpty(x.PhotoBase64))
            .WithMessage("Photo must be valid Base64 encoded data.");
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
