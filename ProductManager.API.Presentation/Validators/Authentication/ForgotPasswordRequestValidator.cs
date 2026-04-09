using ProductManager.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManager.Presentation.Validators.Authentication
{
    public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("E-posta adresi zorunludur.")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(256)
                .WithMessage("E-posta adresi en fazla 256 karakter olabilir.");
        }
    }
}
