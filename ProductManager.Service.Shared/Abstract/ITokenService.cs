using ProductManager.Shared.Dtos.Authentication;
using System.Security.Claims;

namespace ProductManager.Service.Shared.Abstract
{
    public interface ITokenService
    {
        TokenResponseDto GenerateAccessToken(
            Guid userId,
            string email,
            IEnumerable<string> roles,
            IDictionary<string, string>? additionalClaims = null,
            bool rememberMe = false,
            IEnumerable<string>? permissions = null);

        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
    }
}
