using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.SystemManagement;
using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.Service.Concrete
{
    public sealed class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemManagementRepository _repository;

        public SystemSettingsService(ISystemManagementRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
            => _repository.GetSettingsAsync(cancellationToken);

        public async Task BulkUpdateAsync(BulkUpdateSystemSettingsRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.Items.Count == 0)
            {
                return;
            }

            var current = await _repository.GetSettingsAsync(cancellationToken);
            var currentById = current.ToDictionary(s => s.Id);

            foreach (var item in request.Items)
            {
                if (!currentById.TryGetValue(item.Id, out var setting))
                {
                    throw new NotFoundException("Sistem ayarı", item.Id);
                }

                if (!setting.IsEditable)
                {
                    throw new BadRequestException($"\"{setting.DisplayName}\" ayarı düzenlenemez.");
                }
            }

            await _repository.BulkUpdateSettingsAsync(request.Items, cancellationToken);
        }
    }
}
