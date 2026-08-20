using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Entities;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.Identity;
using ProductManagement.Shared.Infrastructure.Exceptions;
using ProductManagement.Shared.Infrastructure.Security;
using System.Security.Claims;

namespace ProductManagement.Service.Concrete
{
    public sealed class RoleManagementService : IRoleManagementService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleManagementService> _logger;

        public RoleManagementService(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RoleManagementService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            var result = new List<RoleDto>(roles.Count);

            foreach (var role in roles)
            {
                result.Add(await MapToDtoAsync(role));
            }

            return result;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            return role is null ? null : await MapToDtoAsync(role);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
        {
            if (await _roleManager.RoleExistsAsync(request.Name))
            {
                throw new ConflictException("Bu isimde bir rol zaten mevcut.");
            }

            var role = new ApplicationRole(request.Name)
            {
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createResult = await _roleManager.CreateAsync(role);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                throw new BadRequestException("Rol oluşturulamadı.", new Dictionary<string, string[]> { ["general"] = errors.ToArray() });
            }

            foreach (var permission in ResolveValidPermissions(request.Permissions))
            {
                await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
            }

            _logger.LogInformation("Yeni rol oluşturuldu. RoleId: {RoleId}, Name: {Name}", role.Id, role.Name);

            return await MapToDtoAsync(role);
        }

        public async Task<bool> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto request, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return false;
            }

            role.Description = request.Description;
            role.IsActive = request.IsActive;
            role.UpdatedAt = DateTime.Now;

            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                throw new BadRequestException("Rol güncellenemedi.", new Dictionary<string, string[]> { ["general"] = errors.ToArray() });
            }

            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var currentPermissions = currentClaims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToList();

            var targetPermissions = ResolveValidPermissions(request.Permissions).ToList();

            var toRemove = currentPermissions.Except(targetPermissions, StringComparer.OrdinalIgnoreCase);
            foreach (var permission in toRemove)
            {
                var claim = currentClaims.First(c => c.Type == Permissions.ClaimType && c.Value == permission);
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            var toAdd = targetPermissions.Except(currentPermissions, StringComparer.OrdinalIgnoreCase);
            foreach (var permission in toAdd)
            {
                await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
            }

            _logger.LogInformation("Rol güncellendi. RoleId: {RoleId}", role.Id);

            return true;
        }

        public async Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return false;
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Count > 0)
            {
                throw new ConflictException("Bu role atanmış kullanıcılar var, önce onları başka bir role taşıyın.");
            }

            var deleteResult = await _roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
            {
                var errors = deleteResult.Errors.Select(e => e.Description);
                throw new BadRequestException("Rol silinemedi.", new Dictionary<string, string[]> { ["general"] = errors.ToArray() });
            }

            _logger.LogInformation("Rol silindi. RoleId: {RoleId}", roleId);

            return true;
        }

        public IReadOnlyList<PermissionDefinitionDto> GetPermissionCatalog()
        {
            return Permissions.All
                .Select(p => new PermissionDefinitionDto { Key = p.Key, DisplayName = p.DisplayName, Category = p.Category })
                .ToList();
        }

        #region Private Methods

        private static IEnumerable<string> ResolveValidPermissions(IEnumerable<string> requested)
            => requested.Distinct(StringComparer.OrdinalIgnoreCase).Where(p => Permissions.AllKeys.Contains(p, StringComparer.OrdinalIgnoreCase));

        private async Task<RoleDto> MapToDtoAsync(ApplicationRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).ToList();
            var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsActive = role.IsActive,
                UserCount = userCount,
                Permissions = permissions,
                CreatedAt = role.CreatedAt
            };
        }

        #endregion
    }
}
