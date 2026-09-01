using FluentValidation;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Validators.ProductOperations
{
    public sealed class CreateProductUnitRequestDtoValidator : AbstractValidator<CreateProductUnitRequestDto>
    {
        public CreateProductUnitRequestDtoValidator()
        {
            RuleFor(x => x.UnitDefinitionId)
                .NotEmpty().WithMessage("UnitDefinitionId is required.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Unit code is required.")
                .MaximumLength(64).WithMessage("Unit code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(250).WithMessage("Unit name max length is 250.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }

    public sealed class UpdateProductUnitRequestDtoValidator : AbstractValidator<UpdateProductUnitRequestDto>
    {
        public UpdateProductUnitRequestDtoValidator()
        {
            RuleFor(x => x.UnitDefinitionId)
                .NotEmpty().WithMessage("UnitDefinitionId is required.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Unit code is required.")
                .MaximumLength(64).WithMessage("Unit code max length is 64.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(250).WithMessage("Unit name max length is 250.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative.");
        }
    }
}
