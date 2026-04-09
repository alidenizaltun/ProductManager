using ProductManager.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManager.Presentation.Validators.Authentication
{
    public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Mevcut şifre zorunludur.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Yeni şifre zorunludur.")
                .MinimumLength(6)
                .WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
                .MaximumLength(128)
                .WithMessage("Yeni şifre en fazla 128 karakter olabilir.")
                .Matches(@"[0-9]")
                .WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("Yeni şifre mevcut şifreden farklı olmalıdır.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Yeni şifre onayı zorunludur.")
                .Equal(x => x.NewPassword)
                .WithMessage("Yeni şifreler eşleşmiyor.");
        }
    }
}
