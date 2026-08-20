using ProductManagement.Shared.Dtos.Authentication;
using FluentValidation;

namespace ProductManagement.Presentation.Validators.Authentication
{
    public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage("Access token zorunludur.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token zorunludur.");
        }
    }
}
