using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Shared.Dtos.SystemManagement;
using ProductManagement.Shared.Infrastructure.Extensions;
using System.Data;

namespace ProductManagement.Repository.Concrete
{
    public sealed class SystemManagementRepository : ISystemManagementRepository
    {
        private readonly string _connectionString;

        public SystemManagementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetActiveConnectionString();
        }

        public async Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Category,
    [Key],
    Value,
    DataType,
    DisplayName,
    Description,
    IsEditable,
    SortOrder,
    UpdatedAt
FROM [System].[SystemSettings]
ORDER BY Category, SortOrder, DisplayName;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<SystemSettingDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<int> BulkUpdateSettingsAsync(IReadOnlyList<UpdateSystemSettingItemDto> items, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [System].[SystemSettings]
SET Value = @Value,
    UpdatedAt = @Now
WHERE Id = @Id
  AND IsEditable = 1;";

            using var connection = CreateConnection();
            var now = DateTime.Now;
            var affected = 0;

            foreach (var item in items)
            {
                affected += await connection.ExecuteAsync(
                    new CommandDefinition(sql, new { item.Id, item.Value, Now = now }, cancellationToken: cancellationToken));
            }

            return affected;
        }

        public async Task<IReadOnlyList<IntegrationRecordDto>> GetIntegrationsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Name,
    Type,
    ProviderKey,
    IsEnabled,
    ConfigJson,
    CredentialsProtected,
    IsSystemManaged,
    Description,
    LastTestedAt,
    LastTestSucceeded,
    LastTestMessage,
    CreatedAt,
    UpdatedAt
FROM [System].[Integrations]
WHERE IsDeleted = 0
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<IntegrationRecordDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<IntegrationRecordDto?> GetIntegrationByIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Name,
    Type,
    ProviderKey,
    IsEnabled,
    ConfigJson,
    CredentialsProtected,
    IsSystemManaged,
    Description,
    LastTestedAt,
    LastTestSucceeded,
    LastTestMessage,
    CreatedAt,
    UpdatedAt
FROM [System].[Integrations]
WHERE Id = @IntegrationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<IntegrationRecordDto>(
                new CommandDefinition(sql, new { IntegrationId = integrationId }, cancellationToken: cancellationToken));
        }

        public async Task<bool> IntegrationExistsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(1) FROM [System].[Integrations];";

            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return count > 0;
        }

        public async Task<IntegrationRecordDto> CreateIntegrationAsync(
            string name,
            string type,
            string providerKey,
            bool isEnabled,
            string? configJson,
            string? credentialsProtected,
            bool isSystemManaged,
            string? description,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [System].[Integrations]
(
    Id,
    Name,
    Type,
    ProviderKey,
    IsEnabled,
    ConfigJson,
    CredentialsProtected,
    IsSystemManaged,
    Description,
    IsDeleted,
    CreatedAt
)
VALUES
(
    @Id,
    @Name,
    @Type,
    @ProviderKey,
    @IsEnabled,
    @ConfigJson,
    @CredentialsProtected,
    @IsSystemManaged,
    @Description,
    0,
    @Now
);";

            var id = Guid.NewGuid();
            var now = DateTime.Now;

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = id,
                        Name = name,
                        Type = type,
                        ProviderKey = providerKey,
                        IsEnabled = isEnabled,
                        ConfigJson = configJson,
                        CredentialsProtected = credentialsProtected,
                        IsSystemManaged = isSystemManaged,
                        Description = description,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            return (await GetIntegrationByIdAsync(id, cancellationToken))!;
        }

        public async Task<bool> UpdateIntegrationAsync(
            Guid integrationId,
            string name,
            bool isEnabled,
            string? configJson,
            string? credentialsProtected,
            bool credentialsProvided,
            string? description,
            CancellationToken cancellationToken = default)
        {
            var sql = credentialsProvided
                ? @"
UPDATE [System].[Integrations]
SET Name = @Name,
    IsEnabled = @IsEnabled,
    ConfigJson = @ConfigJson,
    CredentialsProtected = @CredentialsProtected,
    Description = @Description,
    UpdatedAt = @Now
WHERE Id = @IntegrationId
  AND IsDeleted = 0;"
                : @"
UPDATE [System].[Integrations]
SET Name = @Name,
    IsEnabled = @IsEnabled,
    ConfigJson = @ConfigJson,
    Description = @Description,
    UpdatedAt = @Now
WHERE Id = @IntegrationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        IntegrationId = integrationId,
                        Name = name,
                        IsEnabled = isEnabled,
                        ConfigJson = configJson,
                        CredentialsProtected = credentialsProtected,
                        Description = description,
                        Now = DateTime.Now
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> UpdateIntegrationTestResultAsync(Guid integrationId, bool succeeded, string message, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [System].[Integrations]
SET LastTestedAt = @Now,
    LastTestSucceeded = @Succeeded,
    LastTestMessage = @Message
WHERE Id = @IntegrationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { IntegrationId = integrationId, Now = DateTime.Now, Succeeded = succeeded, Message = message },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [System].[Integrations]
SET IsDeleted = 1,
    DeletedAt = @Now
WHERE Id = @IntegrationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { IntegrationId = integrationId, Now = DateTime.Now }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
