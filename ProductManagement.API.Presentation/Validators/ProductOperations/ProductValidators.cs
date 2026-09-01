using FluentValidation;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Validators.ProductOperations
{
    public sealed class CreateProductRequestDtoValidator : AbstractValidator<CreateProductRequestDto>
    {
        public CreateProductRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.ProductCode)
                .MaximumLength(64).WithMessage("Product code max length is 64.")
                .When(x => !string.IsNullOrWhiteSpace(x.ProductCode));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(250).WithMessage("Product name max length is 250.");

            RuleFor(x => x.DefaultCurrencyCode)
                .NotEmpty().WithMessage("Default currency code is required.")
                .Length(3).WithMessage("Default currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x.Kind)
                .Must(BeValidProductKind).WithMessage("Invalid product kind.");

            RuleFor(x => x.Status)
                .Must(BeValidProductStatus).WithMessage("Invalid product status.");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).When(x => x.TaxRate.HasValue)
                .WithMessage("Tax rate cannot be negative.");
        }

        private static bool BeValidProductKind(int value) => value is 1 or 2 or 3 or 4 or 5 or 6 or 99;
        private static bool BeValidProductStatus(int value) => value is 1 or 2 or 3 or 4;
    }

    public sealed class UpdateProductRequestDtoValidator : AbstractValidator<UpdateProductRequestDto>
    {
        public UpdateProductRequestDtoValidator()
        {
            RuleFor(x => x.ProductCode)
                .NotEmpty().WithMessage("Product code is required.")
                .MaximumLength(64).WithMessage("Product code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(250).WithMessage("Product name max length is 250.");

            RuleFor(x => x.DefaultCurrencyCode)
                .NotEmpty().WithMessage("Default currency code is required.")
                .Length(3).WithMessage("Default currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x.Kind)
                .Must(value => value is 1 or 2 or 3 or 4 or 5 or 6 or 99)
                .WithMessage("Invalid product kind.");

            RuleFor(x => x.Status)
                .Must(value => value is 1 or 2 or 3 or 4)
                .WithMessage("Invalid product status.");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).When(x => x.TaxRate.HasValue)
                .WithMessage("Tax rate cannot be negative.");
        }
    }

    public sealed class CreateProductVariantRequestDtoValidator : AbstractValidator<CreateProductVariantRequestDto>
    {
        public CreateProductVariantRequestDtoValidator()
        {
            RuleFor(x => x.Sku)
                           .NotEmpty().WithMessage("SKU is required.");

            RuleFor(x => x.AdditionalPrice)
                .GreaterThanOrEqualTo(0).When(x => x.AdditionalPrice.HasValue)
                .WithMessage("Additional price cannot be negative.");

            RuleFor(x => x.AdditionalCost)
                .GreaterThanOrEqualTo(0).When(x => x.AdditionalCost.HasValue)
                .WithMessage("Additional cost cannot be negative.");
        }
    }

    public sealed class UpdateProductVariantRequestDtoValidator : AbstractValidator<UpdateProductVariantRequestDto>
    {
        public UpdateProductVariantRequestDtoValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required.");

            RuleFor(x => x.AdditionalPrice)
                .GreaterThanOrEqualTo(0).When(x => x.AdditionalPrice.HasValue)
                .WithMessage("Additional price cannot be negative.");

            RuleFor(x => x.AdditionalCost)
                .GreaterThanOrEqualTo(0).When(x => x.AdditionalCost.HasValue)
                .WithMessage("Additional cost cannot be negative.");
        }
    }

    public sealed class CreateProductPriceRequestDtoValidator : AbstractValidator<CreateProductPriceRequestDto>
    {
        public CreateProductPriceRequestDtoValidator()
        {
            RuleFor(x => x.PriceType)
                           .Must(value => value is >= 1 and <= 5)
                           .WithMessage("Invalid price type.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

            RuleFor(x => x.CompareAtAmount)
                .GreaterThanOrEqualTo(0).When(x => x.CompareAtAmount.HasValue)
                .WithMessage("Compare amount cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x)
                .Must(x => !x.MinQuantity.HasValue || !x.MaxQuantity.HasValue || x.MaxQuantity.Value >= x.MinQuantity.Value)
                .WithMessage("MaxQuantity must be greater than or equal to MinQuantity.");
        }
    }

    public sealed class UpdateProductPriceRequestDtoValidator : AbstractValidator<UpdateProductPriceRequestDto>
    {
        public UpdateProductPriceRequestDtoValidator()
        {
            RuleFor(x => x.PriceType)
                .Must(value => value is >= 1 and <= 5)
                .WithMessage("Invalid price type.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

            RuleFor(x => x.CompareAtAmount)
                .GreaterThanOrEqualTo(0).When(x => x.CompareAtAmount.HasValue)
                .WithMessage("Compare amount cannot be negative.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("Currency code must be uppercase ISO format.");

            RuleFor(x => x)
                .Must(x => !x.MinQuantity.HasValue || !x.MaxQuantity.HasValue || x.MaxQuantity.Value >= x.MinQuantity.Value)
                .WithMessage("MaxQuantity must be greater than or equal to MinQuantity.");
        }
    }

    public sealed class CreateProductInventoryRequestDtoValidator : AbstractValidator<CreateProductInventoryRequestDto>
    {
        public CreateProductInventoryRequestDtoValidator()
        {
            RuleFor(x => x.WarehouseCode)
                           .NotEmpty().WithMessage("Warehouse code is required.");

            RuleFor(x => x.QuantityOnHand)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity on hand cannot be negative.");

            RuleFor(x => x.QuantityReserved)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity reserved cannot be negative.");

            RuleFor(x => x)
                .Must(x => x.QuantityReserved <= x.QuantityOnHand)
                .WithMessage("Quantity reserved cannot be greater than quantity on hand.");

            RuleFor(x => x.ReorderPoint)
                .GreaterThanOrEqualTo(0).When(x => x.ReorderPoint.HasValue)
                .WithMessage("Reorder point cannot be negative.");

            RuleFor(x => x.ReorderQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.ReorderQuantity.HasValue)
                .WithMessage("Reorder quantity cannot be negative.");

            RuleFor(x => x.InventoryPolicy)
                .Must(value => value is >= 1 and <= 3)
                .WithMessage("Invalid inventory policy.");
        }
    }

    public sealed class UpdateProductInventoryRequestDtoValidator : AbstractValidator<UpdateProductInventoryRequestDto>
    {
        public UpdateProductInventoryRequestDtoValidator()
        {
            RuleFor(x => x.WarehouseCode)
                .NotEmpty().WithMessage("Warehouse code is required.");

            RuleFor(x => x.QuantityOnHand)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity on hand cannot be negative.");

            RuleFor(x => x.QuantityReserved)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity reserved cannot be negative.");

            RuleFor(x => x)
                .Must(x => x.QuantityReserved <= x.QuantityOnHand)
                .WithMessage("Quantity reserved cannot be greater than quantity on hand.");

            RuleFor(x => x.ReorderPoint)
                .GreaterThanOrEqualTo(0).When(x => x.ReorderPoint.HasValue)
                .WithMessage("Reorder point cannot be negative.");

            RuleFor(x => x.ReorderQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.ReorderQuantity.HasValue)
                .WithMessage("Reorder quantity cannot be negative.");

            RuleFor(x => x.InventoryPolicy)
                .Must(value => value is >= 1 and <= 3)
                .WithMessage("Invalid inventory policy.");
        }
    }

    public sealed class CreateProductFullRequestDtoValidator : AbstractValidator<CreateProductFullRequestDto>
    {
        public CreateProductFullRequestDtoValidator()
        {
            RuleFor(x => x.Product).SetValidator(new CreateProductRequestDtoValidator());

            RuleForEach(x => x.AttributeValues).SetValidator(new CreateProductAttributeValueRequestDtoValidator());
            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantRequestDtoValidator());
            RuleForEach(x => x.Prices).SetValidator(new CreateProductPriceRequestDtoValidator());
            RuleForEach(x => x.PricingRules).SetValidator(new CreateProductPricingRuleRequestDtoValidator());
            RuleForEach(x => x.ProductUnits).SetValidator(new CreateProductUnitRequestDtoValidator());
            RuleForEach(x => x.Regions).SetValidator(new CreateProductRegionRequestDtoValidator());
            RuleForEach(x => x.Inventories).SetValidator(new CreateProductInventoryRequestDtoValidator());
            RuleForEach(x => x.MediaItems).SetValidator(new CreateProductMediaRequestDtoValidator());
            RuleForEach(x => x.CategoryMaps).SetValidator(new CreateProductCategoryMapRequestDtoValidator());
            RuleForEach(x => x.BundleItems).SetValidator(new CreateProductBundleItemRequestDtoValidator());
            RuleForEach(x => x.SupplierMaps).SetValidator(new CreateProductSupplierMapRequestDtoValidator());
            RuleForEach(x => x.InventoryTransactions).SetValidator(new CreateInventoryTransactionRequestDtoValidator());
            RuleForEach(x => x.InventoryReservations).SetValidator(new CreateInventoryReservationRequestDtoValidator());
            RuleForEach(x => x.PriceListItems).SetValidator(new CreateProductPriceListItemRequestDtoValidator());
            RuleForEach(x => x.Modules).SetValidator(new CreateProductModuleRequestDtoValidator());
            RuleForEach(x => x.LicenseOfferings).SetValidator(new CreateProductLicenseOfferingRequestDtoValidator());

            RuleFor(x => x.PhysicalProfile).SetValidator(new UpsertProductPhysicalProfileRequestDtoValidator());
            RuleFor(x => x.SoftwareProfile).SetValidator(new UpsertProductSoftwareProfileRequestDtoValidator());
            RuleFor(x => x.ServiceProfile).SetValidator(new UpsertProductServiceProfileRequestDtoValidator());
            RuleFor(x => x.SubscriptionProfile).SetValidator(new UpsertProductSubscriptionProfileRequestDtoValidator());
        }
    }

    public sealed class UpdateProductFullRequestDtoValidator : AbstractValidator<UpdateProductFullRequestDto>
    {
        public UpdateProductFullRequestDtoValidator()
        {
            RuleFor(x => x.Product).SetValidator(new UpdateProductRequestDtoValidator());

            RuleForEach(x => x.AttributeValues).SetValidator(new CreateProductAttributeValueRequestDtoValidator());
            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantRequestDtoValidator());
            RuleForEach(x => x.Prices).SetValidator(new CreateProductPriceRequestDtoValidator());
            RuleForEach(x => x.PricingRules).SetValidator(new CreateProductPricingRuleRequestDtoValidator());
            RuleForEach(x => x.ProductUnits).SetValidator(new CreateProductUnitRequestDtoValidator());
            RuleForEach(x => x.Regions).SetValidator(new CreateProductRegionRequestDtoValidator());
            RuleForEach(x => x.Inventories).SetValidator(new CreateProductInventoryRequestDtoValidator());
            RuleForEach(x => x.MediaItems).SetValidator(new CreateProductMediaRequestDtoValidator());
            RuleForEach(x => x.CategoryMaps).SetValidator(new CreateProductCategoryMapRequestDtoValidator());
            RuleForEach(x => x.BundleItems).SetValidator(new CreateProductBundleItemRequestDtoValidator());
            RuleForEach(x => x.SupplierMaps).SetValidator(new CreateProductSupplierMapRequestDtoValidator());
            RuleForEach(x => x.InventoryTransactions).SetValidator(new CreateInventoryTransactionRequestDtoValidator());
            RuleForEach(x => x.InventoryReservations).SetValidator(new CreateInventoryReservationRequestDtoValidator());
            RuleForEach(x => x.PriceListItems).SetValidator(new CreateProductPriceListItemRequestDtoValidator());
            RuleForEach(x => x.Modules).SetValidator(new CreateProductModuleRequestDtoValidator());
            RuleForEach(x => x.LicenseOfferings).SetValidator(new CreateProductLicenseOfferingRequestDtoValidator());

            RuleFor(x => x.PhysicalProfile).SetValidator(new UpsertProductPhysicalProfileRequestDtoValidator());
            RuleFor(x => x.SoftwareProfile).SetValidator(new UpsertProductSoftwareProfileRequestDtoValidator());
            RuleFor(x => x.ServiceProfile).SetValidator(new UpsertProductServiceProfileRequestDtoValidator());
            RuleFor(x => x.SubscriptionProfile).SetValidator(new UpsertProductSubscriptionProfileRequestDtoValidator());
        }
    }
}
