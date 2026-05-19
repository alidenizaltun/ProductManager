using FluentValidation;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Validators.ProductOperations
{
    public sealed class CreateProductAttributeDefinitionRequestDtoValidator : AbstractValidator<CreateProductAttributeDefinitionRequestDto>
    {
        public CreateProductAttributeDefinitionRequestDtoValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Attribute key is required.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required.");

            RuleFor(x => x.DataType)
                .Must(value => value is >= 1 and <= 10)
                .WithMessage("Invalid attribute data type.");
        }
    }

    public sealed class UpdateProductAttributeDefinitionRequestDtoValidator : AbstractValidator<UpdateProductAttributeDefinitionRequestDto>
    {
        public UpdateProductAttributeDefinitionRequestDtoValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Attribute key is required.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required.");

            RuleFor(x => x.DataType)
                .Must(value => value is >= 1 and <= 10)
                .WithMessage("Invalid attribute data type.");
        }
    }

    public sealed class CreateProductAttributeValueRequestDtoValidator : AbstractValidator<CreateProductAttributeValueRequestDto>
    {
        public CreateProductAttributeValueRequestDtoValidator()
        {
            RuleFor(x => x.AttributeDefinitionId)
                           .NotEmpty().WithMessage("Attribute definition id is required.");

            RuleFor(x => x)
                .Must(HasAtLeastOneValue)
                .WithMessage("At least one attribute value field must be provided.");
        }

        private static bool HasAtLeastOneValue(CreateProductAttributeValueRequestDto dto)
            => !string.IsNullOrWhiteSpace(dto.ValueText)
            || dto.ValueNumber.HasValue
            || dto.ValueBool.HasValue
            || dto.ValueDate.HasValue
            || !string.IsNullOrWhiteSpace(dto.ValueJson);
    }

    public sealed class UpdateProductAttributeValueRequestDtoValidator : AbstractValidator<UpdateProductAttributeValueRequestDto>
    {
        public UpdateProductAttributeValueRequestDtoValidator()
        {
            RuleFor(x => x)
                .Must(HasAtLeastOneValue)
                .WithMessage("At least one attribute value field must be provided.");
        }

        private static bool HasAtLeastOneValue(UpdateProductAttributeValueRequestDto dto)
            => !string.IsNullOrWhiteSpace(dto.ValueText)
            || dto.ValueNumber.HasValue
            || dto.ValueBool.HasValue
            || dto.ValueDate.HasValue
            || !string.IsNullOrWhiteSpace(dto.ValueJson);
    }

    public sealed class CreateProductCategoryRequestDtoValidator : AbstractValidator<CreateProductCategoryRequestDto>
    {
        public CreateProductCategoryRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Category code is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.");
        }
    }

    public sealed class UpdateProductCategoryRequestDtoValidator : AbstractValidator<UpdateProductCategoryRequestDto>
    {
        public UpdateProductCategoryRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Category code is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.");
        }
    }

    public sealed class CreateProductCategoryMapRequestDtoValidator : AbstractValidator<CreateProductCategoryMapRequestDto>
    {
        public CreateProductCategoryMapRequestDtoValidator()
        {
            RuleFor(x => x.ProductCategoryId)
                           .NotEmpty().WithMessage("Product category id is required.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class UpdateProductCategoryMapRequestDtoValidator : AbstractValidator<UpdateProductCategoryMapRequestDto>
    {
        public UpdateProductCategoryMapRequestDtoValidator()
        {
            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class CreateProductMediaRequestDtoValidator : AbstractValidator<CreateProductMediaRequestDto>
    {
        public CreateProductMediaRequestDtoValidator()
        {
            RuleFor(x => x.MediaType)
                           .Must(value => value is >= 1 and <= 4)
                           .WithMessage("Invalid media type.");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Media URL is required.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class UpdateProductMediaRequestDtoValidator : AbstractValidator<UpdateProductMediaRequestDto>
    {
        public UpdateProductMediaRequestDtoValidator()
        {
            RuleFor(x => x.MediaType)
                .Must(value => value is >= 1 and <= 4)
                .WithMessage("Invalid media type.");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Media URL is required.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class CreateProductBundleItemRequestDtoValidator : AbstractValidator<CreateProductBundleItemRequestDto>
    {
        public CreateProductBundleItemRequestDtoValidator()
        {
            RuleFor(x => x.BundleProductId)
                .NotEmpty().WithMessage("Bundle product id is required.");

            RuleFor(x => x.ChildProductId)
                .NotEmpty().WithMessage("Child product id is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }

    public sealed class UpdateProductBundleItemRequestDtoValidator : AbstractValidator<UpdateProductBundleItemRequestDto>
    {
        public UpdateProductBundleItemRequestDtoValidator()
        {
            RuleFor(x => x.ChildProductId)
                .NotEmpty().WithMessage("Child product id is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
