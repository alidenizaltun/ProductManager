using ProductManagement.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManagement.Presentation.Validators.Authentication
{
    public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("E-posta adresi zorunludur.")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(256)
                .WithMessage("E-posta adresi en fazla 256 karakter olabilir.");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Şifre sıfırlama token'ı zorunludur.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Yeni şifre zorunludur.")
                .MinimumLength(6)
                .WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
                .MaximumLength(128)
                .WithMessage("Yeni şifre en fazla 128 karakter olabilir.")
                .Matches(@"[0-9]")
                .WithMessage("Yeni şifre en az bir rakam içermelidir.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Yeni şifre onayı zorunludur.")
                .Equal(x => x.NewPassword)
                .WithMessage("Yeni şifreler eşleşmiyor.");
        }
    }
}
