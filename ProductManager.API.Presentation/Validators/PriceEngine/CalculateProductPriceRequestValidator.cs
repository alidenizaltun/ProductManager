using FluentValidation;
using ProductManager.Shared.Dtos.PriceEngine;

namespace ProductManager.Presentation.Validators.PriceEngine;

public sealed class CalculateProductPriceRequestValidator : AbstractValidator<CalculateProductPriceRequestDto>
{
    public CalculateProductPriceRequestValidator()
    {
        RuleFor(x => x.LicenseOfferingId)
            .NotEmpty()
            .When(x => x.OfferingUnits is { Count: > 0 })
            .WithMessage("offeringUnits gönderildiğinde licenseOfferingId zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleForEach(x => x.OfferingUnits).ChildRules(unit =>
        {
            unit.RuleFor(u => u.UnitDefinitionId)
                .NotEmpty().WithMessage("UnitDefinitionId is required.");

            unit.RuleFor(u => u.Value)
                .GreaterThan(0).WithMessage("Birim parametresi değeri sıfırdan büyük olmalıdır.");
        }).When(x => x.OfferingUnits is { Count: > 0 });

        RuleFor(x => x.TaxRateOverride)
            .InclusiveBetween(0, 100).When(x => x.TaxRateOverride.HasValue)
            .WithMessage("Tax rate must be between 0 and 100.");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).When(x => x.DiscountPercent.HasValue)
            .WithMessage("Discount percent must be between 0 and 100.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue)
            .WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x)
            .Must(x => !(x.DiscountPercent.HasValue && x.DiscountAmount.HasValue))
            .WithMessage("DiscountPercent and DiscountAmount cannot be used together.");
    }
}
