using FluentValidation;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Validators.ProductOperations
{
    public sealed class CreateRegionRequestDtoValidator : AbstractValidator<CreateRegionRequestDto>
    {
        public CreateRegionRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.Code)
                .MaximumLength(32).WithMessage("Region code max length is 32.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Region name is required.")
                .MaximumLength(150).WithMessage("Region name max length is 150.");
        }
    }

    public sealed class UpdateRegionRequestDtoValidator : AbstractValidator<UpdateRegionRequestDto>
    {
        public UpdateRegionRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Region code is required.")
                .MaximumLength(32).WithMessage("Region code max length is 32.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Region name is required.")
                .MaximumLength(150).WithMessage("Region name max length is 150.");
        }
    }

    public sealed class CreateProductRegionRequestDtoValidator : AbstractValidator<CreateProductRegionRequestDto>
    {
        public CreateProductRegionRequestDtoValidator()
        {
            // ProductId bilerek dogrulanmiyor: tekil uc (POST products/{productId}/regions)
            // gelen govdeyi "request with { ProductId = productId }" ile eziyor, tam urun
            // kaydinda ise bolgeler alt nesne olarak gidip rotadaki productId ile yaziliyor.
            // Yani deger istemciden hic okunmuyor; zorunlu tutmak bolge atamasini
            // gereksiz yere 400 ile reddediyordu.

            RuleFor(x => x.RegionId)
                .NotEmpty().WithMessage("Region id is required.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.");

            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 100).When(x => x.TaxRate.HasValue)
                .WithMessage("Tax rate must be between 0 and 100.");
        }
    }

    public sealed class UpdateProductRegionRequestDtoValidator : AbstractValidator<UpdateProductRegionRequestDto>
    {
        public UpdateProductRegionRequestDtoValidator()
        {
            RuleFor(x => x.RegionId)
                .NotEmpty().WithMessage("Region id is required.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be 3 characters.");

            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 100).When(x => x.TaxRate.HasValue)
                .WithMessage("Tax rate must be between 0 and 100.");
        }
    }
}
