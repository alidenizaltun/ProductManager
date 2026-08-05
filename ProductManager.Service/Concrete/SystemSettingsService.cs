using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.SystemManagement;
using ProductManager.Shared.Infrastructure.Exceptions;

namespace ProductManager.Service.Concrete
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
