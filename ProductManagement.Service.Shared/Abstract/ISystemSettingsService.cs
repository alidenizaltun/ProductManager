using ProductManagement.Shared.Dtos.SystemManagement;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface ISystemSettingsService
    {
        Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task BulkUpdateAsync(BulkUpdateSystemSettingsRequestDto request, CancellationToken cancellationToken = default);
    }
}
