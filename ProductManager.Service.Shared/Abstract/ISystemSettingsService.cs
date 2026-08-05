using ProductManager.Shared.Dtos.SystemManagement;

namespace ProductManager.Service.Shared.Abstract
{
    public interface ISystemSettingsService
    {
        Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task BulkUpdateAsync(BulkUpdateSystemSettingsRequestDto request, CancellationToken cancellationToken = default);
    }
}
