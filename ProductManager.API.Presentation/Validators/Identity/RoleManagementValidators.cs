using FluentValidation;
using ProductManager.Shared.Dtos.Identity;

namespace ProductManager.Presentation.Validators.Identity
{
    public sealed class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
    {
        public CreateRoleRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Rol adı zorunludur.")
                .MaximumLength(256).WithMessage("Rol adı en fazla 256 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
        }
    }

    public sealed class UpdateRoleRequestDtoValidator : AbstractValidator<UpdateRoleRequestDto>
    {
        public UpdateRoleRequestDtoValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
        }
    }
}
