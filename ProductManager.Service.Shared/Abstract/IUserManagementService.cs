using ProductManager.Shared.Dtos.Identity;

namespace ProductManager.Service.Shared.Abstract
{
    public interface IUserManagementService
    {
        Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default);
        Task<AdminUserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<AdminUserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ResendInvitationAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
