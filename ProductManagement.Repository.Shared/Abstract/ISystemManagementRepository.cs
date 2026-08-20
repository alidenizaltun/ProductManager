using ProductManagement.Shared.Dtos.SystemManagement;

namespace ProductManagement.Repository.Shared.Abstract
{
    public interface ISystemManagementRepository
    {
        Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task<int> BulkUpdateSettingsAsync(IReadOnlyList<UpdateSystemSettingItemDto> items, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<IntegrationRecordDto>> GetIntegrationsAsync(CancellationToken cancellationToken = default);
        Task<IntegrationRecordDto?> GetIntegrationByIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
        Task<bool> IntegrationExistsAsync(CancellationToken cancellationToken = default);
        Task<IntegrationRecordDto> CreateIntegrationAsync(
            string name,
            string type,
            string providerKey,
            bool isEnabled,
            string? configJson,
            string? credentialsProtected,
            bool isSystemManaged,
            string? description,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateIntegrationAsync(
            Guid integrationId,
            string name,
            bool isEnabled,
            string? configJson,
            string? credentialsProtected,
            bool credentialsProvided,
            string? description,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateIntegrationTestResultAsync(Guid integrationId, bool succeeded, string message, CancellationToken cancellationToken = default);
        Task<bool> DeleteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    }
}
