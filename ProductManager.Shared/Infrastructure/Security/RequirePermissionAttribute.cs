using Microsoft.AspNetCore.Authorization;

namespace ProductManager.Shared.Infrastructure.Security
{
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = $"{PermissionPolicyProvider.PolicyPrefix}{permission}";
        }
    }
}
