using ProductManagement.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManagement.Presentation.Validators.Authentication
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("E-posta adresi zorunludur.")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(256)
                .WithMessage("E-posta adresi en fazla 256 karakter olabilir.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Şifre zorunludur.")
                .MinimumLength(6)
                .WithMessage("Şifre en az 6 karakter olmalıdır.")
                .MaximumLength(128)
                .WithMessage("Şifre en fazla 128 karakter olabilir.");
        }
    }
}
