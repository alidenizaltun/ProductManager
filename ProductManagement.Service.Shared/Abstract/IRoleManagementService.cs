using ProductManagement.Shared.Dtos.Identity;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface IRoleManagementService
    {
        Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
        IReadOnlyList<PermissionDefinitionDto> GetPermissionCatalog();
    }
}
