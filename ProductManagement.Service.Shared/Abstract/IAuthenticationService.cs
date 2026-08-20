using ProductManagement.Shared.Dtos.Authentication;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface IAuthenticationService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
        Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
        Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    }
}
