using FluentValidation;
using JoiabagurPV.Application.DTOs.Sales;

namespace JoiabagurPV.Application.Validators;

public class CreateBulkSalesRequestValidator : AbstractValidator<CreateBulkSalesRequest>
{
    public CreateBulkSalesRequestValidator()
    {
        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("Point of Sale ID is required.");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Payment Method ID is required.");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one sale line is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required for each line.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.Price)
                .GreaterThan(0)
                .When(l => l.Price.HasValue)
                .WithMessage("Price must be greater than zero.");
        });
    }
}
