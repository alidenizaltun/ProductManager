using FluentValidation;
using ProductManager.Shared.Dtos.SystemManagement;

namespace ProductManager.Presentation.Validators.System
{
    public sealed class CreateIntegrationRequestDtoValidator : AbstractValidator<CreateIntegrationRequestDto>
    {
        public CreateIntegrationRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Entegrasyon adı zorunludur.")
                .MaximumLength(150).WithMessage("Entegrasyon adı en fazla 150 karakter olabilir.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Entegrasyon tipi zorunludur.")
                .MaximumLength(50);

            RuleFor(x => x.ProviderKey)
                .NotEmpty().WithMessage("Sağlayıcı anahtarı zorunludur.")
                .MaximumLength(100);
        }
    }

    public sealed class UpdateIntegrationRequestDtoValidator : AbstractValidator<UpdateIntegrationRequestDto>
    {
        public UpdateIntegrationRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Entegrasyon adı zorunludur.")
                .MaximumLength(150).WithMessage("Entegrasyon adı en fazla 150 karakter olabilir.");
        }
    }

    public sealed class BulkUpdateSystemSettingsRequestDtoValidator : AbstractValidator<BulkUpdateSystemSettingsRequestDto>
    {
        public BulkUpdateSystemSettingsRequestDtoValidator()
        {
            RuleFor(x => x.Items).NotNull();
        }
    }
}
