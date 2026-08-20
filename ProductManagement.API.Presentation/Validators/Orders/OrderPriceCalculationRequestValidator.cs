using FluentValidation;
using ProductManagement.Shared.Dtos.Orders;

namespace ProductManagement.Presentation.Validators.Orders;

public sealed class OrderPriceCalculationRequestValidator : AbstractValidator<OrderPriceCalculationRequestDto>
{
    public OrderPriceCalculationRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
