using ProductManagement.Service.Shared.Abstract;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProductManagement.Service.Concrete
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("uid")
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public IEnumerable<string> Roles =>
            _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();

        public Guid? DealerId
        {
            get
            {
                var dealerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("DealerId");

                if (dealerIdClaim != null && Guid.TryParse(dealerIdClaim.Value, out var dealerId))
                {
                    return dealerId;
                }

                return null;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

        public IEnumerable<Claim> GetClaims() =>
            _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    }
}
