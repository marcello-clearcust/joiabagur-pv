using FluentValidation;
using JoiabagurPV.Application.DTOs.Inventory;

namespace JoiabagurPV.Application.Validators;

/// <summary>
/// Validator for AssignProductRequest.
/// </summary>
public class AssignProductRequestValidator : AbstractValidator<AssignProductRequest>
{
    public AssignProductRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("El ID del punto de venta es requerido.");
    }
}

/// <summary>
/// Validator for BulkAssignProductsRequest.
/// </summary>
public class BulkAssignProductsRequestValidator : AbstractValidator<BulkAssignProductsRequest>
{
    public BulkAssignProductsRequestValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("Al menos un producto es requerido.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("El ID del punto de venta es requerido.");
    }
}

/// <summary>
/// Validator for UnassignProductRequest.
/// </summary>
public class UnassignProductRequestValidator : AbstractValidator<UnassignProductRequest>
{
    public UnassignProductRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.PointOfSaleId)
            .NotEmpty()
            .WithMessage("El ID del punto de venta es requerido.");
    }
}

