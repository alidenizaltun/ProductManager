using System.Security.Claims;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        IEnumerable<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        IEnumerable<Claim> GetClaims();
    }
}
