using ProductManager.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManager.Presentation.Validators.Authentication
{
    public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
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
                .WithMessage("Şifre en fazla 128 karakter olabilir.")
                .Matches(@"[0-9]")
                .WithMessage("Şifre en az bir rakam içermelidir.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Şifre onayı zorunludur.")
                .Equal(x => x.Password)
                .WithMessage("Şifreler eşleşmiyor.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("Ad zorunludur.")
                .MaximumLength(100)
                .WithMessage("Ad en fazla 100 karakter olabilir.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$")
                .WithMessage("Ad sadece harf içermelidir.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Soyad zorunludur.")
                .MaximumLength(100)
                .WithMessage("Soyad en fazla 100 karakter olabilir.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$")
                .WithMessage("Soyad sadece harf içermelidir.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Geçerli bir telefon numarası giriniz.");
        }
    }
}
