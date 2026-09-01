using FluentValidation;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Validators.ProductOperations
{
    public sealed class CreateProductModuleRequestDtoValidator : AbstractValidator<CreateProductModuleRequestDto>
    {
        public CreateProductModuleRequestDtoValidator()
        {
            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("Module code is required.")
                .MaximumLength(64).WithMessage("Module code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Module name is required.")
                .MaximumLength(250).WithMessage("Module name max length is 250.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");

            RuleForEach(x => x.OfferingPrices).SetValidator(new CreateModuleOfferingPriceInlineDtoValidator());
        }
    }

    public sealed class UpdateProductModuleRequestDtoValidator : AbstractValidator<UpdateProductModuleRequestDto>
    {
        public UpdateProductModuleRequestDtoValidator()
        {
            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("Module code is required.")
                .MaximumLength(64).WithMessage("Module code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Module name is required.")
                .MaximumLength(250).WithMessage("Module name max length is 250.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class CreateModuleOfferingPriceInlineDtoValidator : AbstractValidator<CreateModuleOfferingPriceInlineDto>
    {
        public CreateModuleOfferingPriceInlineDtoValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x)
                .Must(x => x.ProductLicenseOfferingId.HasValue || !string.IsNullOrWhiteSpace(x.LicenseOfferingTempId))
                .WithMessage("Either ProductLicenseOfferingId or LicenseOfferingTempId is required.");
        }
    }

    public sealed class CreateProductModuleOfferingPriceRequestDtoValidator : AbstractValidator<CreateProductModuleOfferingPriceRequestDto>
    {
        public CreateProductModuleOfferingPriceRequestDtoValidator()
        {
            RuleFor(x => x.ProductModuleId)
                .NotEmpty().WithMessage("ProductModuleId is required.");

            RuleFor(x => x.ProductLicenseOfferingId)
                .NotEmpty().WithMessage("ProductLicenseOfferingId is required.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");
        }
    }

    public sealed class UpdateProductModuleOfferingPriceRequestDtoValidator : AbstractValidator<UpdateProductModuleOfferingPriceRequestDto>
    {
        public UpdateProductModuleOfferingPriceRequestDtoValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");
        }
    }

    public sealed class CreateProductLicenseOfferingRequestDtoValidator : AbstractValidator<CreateProductLicenseOfferingRequestDto>
    {
        public CreateProductLicenseOfferingRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("License offering name is required.")
                .MaximumLength(250).WithMessage("License offering name max length is 250.");

            RuleFor(x => x.LicenseModel)
                .InclusiveBetween(1, 5).WithMessage("Invalid license model.");

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x.BillingPeriodUnit)
                .InclusiveBetween(1, 4).When(x => x.BillingPeriodUnit.HasValue)
                .WithMessage("Invalid billing period unit.");

            RuleFor(x => x.BillingPeriodValue)
                .GreaterThan(0).When(x => x.BillingPeriodValue.HasValue)
                .WithMessage("Billing period value must be greater than zero.");

            RuleFor(x => x.GracePeriodDays)
                .GreaterThanOrEqualTo(0).When(x => x.GracePeriodDays.HasValue)
                .WithMessage("Grace period days cannot be negative.");

            RuleFor(x => x.TrialDays)
                .GreaterThanOrEqualTo(0).When(x => x.TrialDays.HasValue)
                .WithMessage("Trial days cannot be negative.");

            RuleFor(x => x.MaxSeats)
                .GreaterThan(0).When(x => x.MaxSeats.HasValue)
                .WithMessage("Max seats must be greater than zero.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("ValidTo must be later than ValidFrom.");
        }
    }

    public sealed class UpdateProductLicenseOfferingRequestDtoValidator : AbstractValidator<UpdateProductLicenseOfferingRequestDto>
    {
        public UpdateProductLicenseOfferingRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("License offering name is required.")
                .MaximumLength(250).WithMessage("License offering name max length is 250.");

            RuleFor(x => x.LicenseModel)
                .InclusiveBetween(1, 5).WithMessage("Invalid license model.");

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x.BillingPeriodUnit)
                .InclusiveBetween(1, 4).When(x => x.BillingPeriodUnit.HasValue)
                .WithMessage("Invalid billing period unit.");

            RuleFor(x => x.BillingPeriodValue)
                .GreaterThan(0).When(x => x.BillingPeriodValue.HasValue)
                .WithMessage("Billing period value must be greater than zero.");

            RuleFor(x => x.GracePeriodDays)
                .GreaterThanOrEqualTo(0).When(x => x.GracePeriodDays.HasValue)
                .WithMessage("Grace period days cannot be negative.");

            RuleFor(x => x.TrialDays)
                .GreaterThanOrEqualTo(0).When(x => x.TrialDays.HasValue)
                .WithMessage("Trial days cannot be negative.");

            RuleFor(x => x.MaxSeats)
                .GreaterThan(0).When(x => x.MaxSeats.HasValue)
                .WithMessage("Max seats must be greater than zero.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("ValidTo must be later than ValidFrom.");
        }
    }
}
