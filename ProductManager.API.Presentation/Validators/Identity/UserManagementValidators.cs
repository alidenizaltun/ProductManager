using FluentValidation;
using ProductManager.Shared.Dtos.Identity;

namespace ProductManager.Presentation.Validators.Identity
{
    public sealed class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
    {
        public CreateUserRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");

            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");
        }
    }

    public sealed class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");
        }
    }
}
