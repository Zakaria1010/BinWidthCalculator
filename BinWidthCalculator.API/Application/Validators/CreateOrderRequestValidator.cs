using FluentValidation;
using BinWidthCalculator.Application.DTOs;

namespace BinWidthCalculator.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            
            item.RuleFor(x => x.ProductType)
                .IsInEnum().WithMessage("Invalid product type");
        });
    }
}