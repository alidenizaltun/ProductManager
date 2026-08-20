using ProductManagement.Shared.Dtos.SystemManagement;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface IIntegrationService
    {
        Task<IReadOnlyList<IntegrationDto>> GetIntegrationsAsync(CancellationToken cancellationToken = default);
        Task<IntegrationDto?> GetIntegrationByIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
        Task<IntegrationDto> CreateIntegrationAsync(CreateIntegrationRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateIntegrationAsync(Guid integrationId, UpdateIntegrationRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
        Task<IntegrationDto?> TestIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    }
}
