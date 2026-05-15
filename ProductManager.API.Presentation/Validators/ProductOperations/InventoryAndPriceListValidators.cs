using FluentValidation;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Validators.ProductOperations
{
    public sealed class CreateInventoryTransactionRequestDtoValidator : AbstractValidator<CreateInventoryTransactionRequestDto>
    {
        public CreateInventoryTransactionRequestDtoValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
            RuleFor(x => x.TransactionType).InclusiveBetween(1, 8).WithMessage("Invalid transaction type.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).When(x => x.UnitCost.HasValue)
                .WithMessage("Unit cost cannot be negative.");
            RuleFor(x => x.ReferenceType).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.ReferenceType));
            RuleFor(x => x.ReferenceNumber).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));
            RuleFor(x => x.Note).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }

    public sealed class CreateInventoryReservationRequestDtoValidator : AbstractValidator<CreateInventoryReservationRequestDto>
    {
        public CreateInventoryReservationRequestDtoValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product id is required.");
            RuleFor(x => x.ReservationCode).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            RuleFor(x => x.Status).InclusiveBetween(1, 4).WithMessage("Invalid reservation status.");
            RuleFor(x => x.SourceType).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.SourceType));
            RuleFor(x => x.SourceId).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.SourceId));
        }
    }

    public sealed class UpdateInventoryReservationStatusRequestDtoValidator : AbstractValidator<UpdateInventoryReservationStatusRequestDto>
    {
        public UpdateInventoryReservationStatusRequestDtoValidator()
        {
            RuleFor(x => x.Status).InclusiveBetween(1, 4).WithMessage("Invalid reservation status.");
        }
    }

    public sealed class CreateProductPriceListRequestDtoValidator : AbstractValidator<CreateProductPriceListRequestDto>
    {
        public CreateProductPriceListRequestDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.CurrencyCode).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
            RuleFor(x => x.SalesChannel).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.SalesChannel));
            RuleFor(x => x.CustomerGroupCode).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.CustomerGroupCode));
            RuleFor(x => x).Must(x => !x.ValidFrom.HasValue || !x.ValidTo.HasValue || x.ValidTo.Value >= x.ValidFrom.Value)
                .WithMessage("ValidTo must be greater than or equal to ValidFrom.");
        }
    }

    public sealed class UpdateProductPriceListRequestDtoValidator : AbstractValidator<UpdateProductPriceListRequestDto>
    {
        public UpdateProductPriceListRequestDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.CurrencyCode).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
            RuleFor(x => x.SalesChannel).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.SalesChannel));
            RuleFor(x => x.CustomerGroupCode).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.CustomerGroupCode));
            RuleFor(x => x).Must(x => !x.ValidFrom.HasValue || !x.ValidTo.HasValue || x.ValidTo.Value >= x.ValidFrom.Value)
                .WithMessage("ValidTo must be greater than or equal to ValidFrom.");
        }
    }

    public sealed class CreateProductPriceListItemRequestDtoValidator : AbstractValidator<CreateProductPriceListItemRequestDto>
    {
        public CreateProductPriceListItemRequestDtoValidator()
        {
            RuleFor(x => x.ProductPriceListId).NotEmpty();
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CompareAtAmount).GreaterThanOrEqualTo(0).When(x => x.CompareAtAmount.HasValue);
            RuleFor(x => x.MinQuantity).GreaterThan(0).When(x => x.MinQuantity.HasValue);
            RuleFor(x => x.MaxQuantity).GreaterThan(0).When(x => x.MaxQuantity.HasValue);
            RuleFor(x => x).Must(x => !x.MinQuantity.HasValue || !x.MaxQuantity.HasValue || x.MaxQuantity.Value >= x.MinQuantity.Value)
                .WithMessage("MaxQuantity must be greater than or equal to MinQuantity.");
        }
    }

    public sealed class UpdateProductPriceListItemRequestDtoValidator : AbstractValidator<UpdateProductPriceListItemRequestDto>
    {
        public UpdateProductPriceListItemRequestDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CompareAtAmount).GreaterThanOrEqualTo(0).When(x => x.CompareAtAmount.HasValue);
            RuleFor(x => x.MinQuantity).GreaterThan(0).When(x => x.MinQuantity.HasValue);
            RuleFor(x => x.MaxQuantity).GreaterThan(0).When(x => x.MaxQuantity.HasValue);
            RuleFor(x => x).Must(x => !x.MinQuantity.HasValue || !x.MaxQuantity.HasValue || x.MaxQuantity.Value >= x.MinQuantity.Value)
                .WithMessage("MaxQuantity must be greater than or equal to MinQuantity.");
        }
    }

    public sealed class UpsertProductPhysicalProfileRequestDtoValidator : AbstractValidator<UpsertProductPhysicalProfileRequestDto>
    {
        public UpsertProductPhysicalProfileRequestDtoValidator()
        {
            RuleFor(x => x.Weight).GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue);
            RuleFor(x => x.Width).GreaterThanOrEqualTo(0).When(x => x.Width.HasValue);
            RuleFor(x => x.Height).GreaterThanOrEqualTo(0).When(x => x.Height.HasValue);
            RuleFor(x => x.Length).GreaterThanOrEqualTo(0).When(x => x.Length.HasValue);
            RuleFor(x => x.WarrantyInMonths).InclusiveBetween(0, 600).When(x => x.WarrantyInMonths.HasValue);
        }
    }

    public sealed class UpsertProductSoftwareProfileRequestDtoValidator : AbstractValidator<UpsertProductSoftwareProfileRequestDto>
    {
        public UpsertProductSoftwareProfileRequestDtoValidator()
        {
            RuleFor(x => x.Version).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Version));
            RuleFor(x => x.DownloadUrl).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.DownloadUrl));
        }
    }

    public sealed class UpsertProductServiceProfileRequestDtoValidator : AbstractValidator<UpsertProductServiceProfileRequestDto>
    {
        public UpsertProductServiceProfileRequestDtoValidator()
        {
            RuleFor(x => x.DeliveryMode).InclusiveBetween(1, 4);
            RuleFor(x => x.DurationInMinutes).GreaterThan(0).When(x => x.DurationInMinutes.HasValue);
            RuleFor(x => x.MaxConcurrentBooking).GreaterThan(0).When(x => x.MaxConcurrentBooking.HasValue);
        }
    }

    public sealed class UpsertProductSubscriptionProfileRequestDtoValidator : AbstractValidator<UpsertProductSubscriptionProfileRequestDto>
    {
        public UpsertProductSubscriptionProfileRequestDtoValidator()
        {
            RuleFor(x => x.BillingPeriodUnit).InclusiveBetween(1, 4);
            RuleFor(x => x.BillingPeriodValue).GreaterThan(0);
            RuleFor(x => x.TrialDays).InclusiveBetween(0, 3650).When(x => x.TrialDays.HasValue);
            RuleFor(x => x.GracePeriodDays).InclusiveBetween(0, 365).When(x => x.GracePeriodDays.HasValue);
            RuleFor(x => x.CancellationPolicy).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.CancellationPolicy));
        }
    }
}
