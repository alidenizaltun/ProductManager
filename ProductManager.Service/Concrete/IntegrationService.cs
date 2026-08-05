using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.SystemManagement;
using ProductManager.Shared.Infrastructure.Exceptions;
using ProductManager.Shared.Infrastructure.Security;
using System.Text.Json;

namespace ProductManager.Service.Concrete
{
    public sealed class IntegrationService : IIntegrationService
    {
        private readonly ISystemManagementRepository _repository;
        private readonly IDataProtector _protector;
        private readonly ILogger<IntegrationService> _logger;

        public IntegrationService(
            ISystemManagementRepository repository,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<IntegrationService> logger)
        {
            _repository = repository;
            _protector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.IntegrationsCredentials);
            _logger = logger;
        }

        public async Task<IReadOnlyList<IntegrationDto>> GetIntegrationsAsync(CancellationToken cancellationToken = default)
        {
            var records = await _repository.GetIntegrationsAsync(cancellationToken);
            return records.Select(MapToDto).ToList();
        }

        public async Task<IntegrationDto?> GetIntegrationByIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
        {
            var record = await _repository.GetIntegrationByIdAsync(integrationId, cancellationToken);
            return record is null ? null : MapToDto(record);
        }

        public async Task<IntegrationDto> CreateIntegrationAsync(CreateIntegrationRequestDto request, CancellationToken cancellationToken = default)
        {
            var credentialsProtected = ProtectCredentials(request.Credentials);

            IntegrationRecordDto created;
            try
            {
                created = await _repository.CreateIntegrationAsync(
                    request.Name,
                    request.Type,
                    request.ProviderKey,
                    request.IsEnabled,
                    request.ConfigJson,
                    credentialsProtected,
                    isSystemManaged: false,
                    request.Description,
                    cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                throw new ConflictException("Bu sağlayıcı anahtarına (ProviderKey) sahip bir entegrasyon zaten mevcut.");
            }

            _logger.LogInformation("Yeni entegrasyon oluşturuldu. IntegrationId: {IntegrationId}", created.Id);

            return MapToDto(created);
        }

        public async Task<bool> UpdateIntegrationAsync(Guid integrationId, UpdateIntegrationRequestDto request, CancellationToken cancellationToken = default)
        {
            var credentialsProvided = request.Credentials is { Count: > 0 };
            var credentialsProtected = credentialsProvided ? ProtectCredentials(request.Credentials) : null;

            var updated = await _repository.UpdateIntegrationAsync(
                integrationId,
                request.Name,
                request.IsEnabled,
                request.ConfigJson,
                credentialsProtected,
                credentialsProvided,
                request.Description,
                cancellationToken);

            if (updated)
            {
                _logger.LogInformation("Entegrasyon güncellendi. IntegrationId: {IntegrationId}", integrationId);
            }

            return updated;
        }

        public async Task<bool> DeleteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default)
        {
            var deleted = await _repository.DeleteIntegrationAsync(integrationId, cancellationToken);

            if (deleted)
            {
                _logger.LogInformation("Entegrasyon silindi. IntegrationId: {IntegrationId}", integrationId);
            }

            return deleted;
        }

        public async Task<IntegrationDto?> TestIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default)
        {
            var record = await _repository.GetIntegrationByIdAsync(integrationId, cancellationToken);
            if (record is null)
            {
                return null;
            }

            var (succeeded, message) = ValidateConfiguration(record);
            await _repository.UpdateIntegrationTestResultAsync(integrationId, succeeded, message, cancellationToken);

            var refreshed = await _repository.GetIntegrationByIdAsync(integrationId, cancellationToken);
            return refreshed is null ? null : MapToDto(refreshed);
        }

        #region Private Methods

        private static (bool Succeeded, string Message) ValidateConfiguration(IntegrationRecordDto record)
        {
            if (string.IsNullOrWhiteSpace(record.ProviderKey))
            {
                return (false, "Sağlayıcı anahtarı (ProviderKey) tanımlı değil.");
            }

            if (!string.IsNullOrWhiteSpace(record.ConfigJson))
            {
                try
                {
                    JsonDocument.Parse(record.ConfigJson);
                }
                catch (JsonException)
                {
                    return (false, "Yapılandırma (ConfigJson) geçerli bir JSON değil.");
                }
            }

            if (string.IsNullOrWhiteSpace(record.CredentialsProtected))
            {
                return (true, "Yapılandırma doğrulandı (kimlik bilgisi tanımlanmamış). Not: Bu bir canlı bağlantı testi değildir.");
            }

            return (true, "Yapılandırma doğrulandı. Not: Bu bir canlı bağlantı testi değildir, sadece kayıtlı alanların biçimini kontrol eder.");
        }

        private string? ProtectCredentials(Dictionary<string, string>? credentials)
        {
            if (credentials is not { Count: > 0 })
            {
                return null;
            }

            var json = JsonSerializer.Serialize(credentials);
            return _protector.Protect(json);
        }

        private Dictionary<string, string>? UnprotectCredentials(string? credentialsProtected)
        {
            if (string.IsNullOrEmpty(credentialsProtected))
            {
                return null;
            }

            try
            {
                var json = _protector.Unprotect(credentialsProtected);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string MaskValue(string value)
        {
            if (value.Length <= 6)
            {
                return new string('*', value.Length);
            }

            return $"{value[..3]}****{value[^3..]}";
        }

        private IntegrationDto MapToDto(IntegrationRecordDto record)
        {
            var credentials = UnprotectCredentials(record.CredentialsProtected);
            string? preview = null;

            if (credentials is { Count: > 0 })
            {
                var first = credentials.First();
                preview = credentials.Count == 1
                    ? $"{first.Key}: {MaskValue(first.Value)}"
                    : $"{first.Key}: {MaskValue(first.Value)} (+{credentials.Count - 1} diğer)";
            }

            return new IntegrationDto
            {
                Id = record.Id,
                Name = record.Name,
                Type = record.Type,
                ProviderKey = record.ProviderKey,
                IsEnabled = record.IsEnabled,
                ConfigJson = record.ConfigJson,
                HasCredentials = credentials is { Count: > 0 },
                CredentialsPreview = preview,
                IsSystemManaged = record.IsSystemManaged,
                Description = record.Description,
                LastTestedAt = record.LastTestedAt,
                LastTestSucceeded = record.LastTestSucceeded,
                LastTestMessage = record.LastTestMessage,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }

        #endregion
    }
}
