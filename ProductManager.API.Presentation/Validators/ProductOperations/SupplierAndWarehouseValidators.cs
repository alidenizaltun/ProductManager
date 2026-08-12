using FluentValidation;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Validators.ProductOperations
{
    public sealed class CreateProductSupplierRequestDtoValidator : AbstractValidator<CreateProductSupplierRequestDto>
    {
        public CreateProductSupplierRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.SupplierCode)
                .MaximumLength(64).WithMessage("Supplier code max length is 64.")
                .When(x => !string.IsNullOrWhiteSpace(x.SupplierCode));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(250).WithMessage("Supplier name max length is 250.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Email format is invalid.");
        }
    }

    public sealed class UpdateProductSupplierRequestDtoValidator : AbstractValidator<UpdateProductSupplierRequestDto>
    {
        public UpdateProductSupplierRequestDtoValidator()
        {
            RuleFor(x => x.SupplierCode)
                .NotEmpty().WithMessage("Supplier code is required.")
                .MaximumLength(64).WithMessage("Supplier code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Supplier name is required.")
                .MaximumLength(250).WithMessage("Supplier name max length is 250.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Email format is invalid.");
        }
    }

    public sealed class CreateProductSupplierMapRequestDtoValidator : AbstractValidator<CreateProductSupplierMapRequestDto>
    {
        public CreateProductSupplierMapRequestDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product id is required.");

            RuleFor(x => x.ProductSupplierId)
                .NotEmpty().WithMessage("Product supplier id is required.");

            RuleFor(x => x.SupplierCost)
                .GreaterThanOrEqualTo(0).When(x => x.SupplierCost.HasValue)
                .WithMessage("Supplier cost cannot be negative.");

            RuleFor(x => x.LeadTimeInDays)
                .GreaterThanOrEqualTo(0).When(x => x.LeadTimeInDays.HasValue)
                .WithMessage("Lead time cannot be negative.");

            RuleFor(x => x.MinOrderQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.MinOrderQuantity.HasValue)
                .WithMessage("Minimum order quantity cannot be negative.");
        }
    }

    public sealed class UpdateProductSupplierMapRequestDtoValidator : AbstractValidator<UpdateProductSupplierMapRequestDto>
    {
        public UpdateProductSupplierMapRequestDtoValidator()
        {
            RuleFor(x => x.SupplierCost)
                .GreaterThanOrEqualTo(0).When(x => x.SupplierCost.HasValue)
                .WithMessage("Supplier cost cannot be negative.");

            RuleFor(x => x.LeadTimeInDays)
                .GreaterThanOrEqualTo(0).When(x => x.LeadTimeInDays.HasValue)
                .WithMessage("Lead time cannot be negative.");

            RuleFor(x => x.MinOrderQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.MinOrderQuantity.HasValue)
                .WithMessage("Minimum order quantity cannot be negative.");
        }
    }

    public sealed class CreateWarehouseRequestDtoValidator : AbstractValidator<CreateWarehouseRequestDto>
    {
        public CreateWarehouseRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.Code)
                .MaximumLength(32).WithMessage("Warehouse code max length is 32.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required.")
                .MaximumLength(150).WithMessage("Warehouse name max length is 150.");
        }
    }

    public sealed class UpdateWarehouseRequestDtoValidator : AbstractValidator<UpdateWarehouseRequestDto>
    {
        public UpdateWarehouseRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Warehouse code is required.")
                .MaximumLength(32).WithMessage("Warehouse code max length is 32.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Warehouse name is required.")
                .MaximumLength(150).WithMessage("Warehouse name max length is 150.");
        }
    }

    public sealed class CreateUnitDefinitionRequestDtoValidator : AbstractValidator<CreateUnitDefinitionRequestDto>
    {
        public CreateUnitDefinitionRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.Code)
                .MaximumLength(32).WithMessage("Unit code max length is 32.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(100).WithMessage("Unit name max length is 100.");
        }
    }

    public sealed class UpdateUnitDefinitionRequestDtoValidator : AbstractValidator<UpdateUnitDefinitionRequestDto>
    {
        public UpdateUnitDefinitionRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Unit code is required.")
                .MaximumLength(32).WithMessage("Unit code max length is 32.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(100).WithMessage("Unit name max length is 100.");
        }
    }
}
