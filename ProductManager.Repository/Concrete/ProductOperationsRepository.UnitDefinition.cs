using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        public async Task<IReadOnlyList<UnitDefinitionDto>> GetUnitDefinitionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var sql = @"
SELECT Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[UnitDefinitions]
WHERE IsDeleted = 0" + (includeInactive ? "" : " AND IsActive = 1") + @"
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<UnitDefinitionDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetUnitDefinitionLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var sql = @"
SELECT Id, Name
FROM [Product].[UnitDefinitions]
WHERE IsDeleted = 0" + (includeInactive ? "" : " AND IsActive = 1") + @"
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<UnitDefinitionDto?> GetUnitDefinitionByIdAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[UnitDefinitions]
WHERE Id = @UnitDefinitionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UnitDefinitionDto>(
                new CommandDefinition(sql, new { UnitDefinitionId = unitDefinitionId }, cancellationToken: cancellationToken));
        }

        public async Task<UnitDefinitionDto> CreateUnitDefinitionAsync(CreateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[UnitDefinitions]
    (Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @Code, @Name, @Description, @IsActive, @SortOrder, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.Code,
                request.Name,
                request.Description,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetUnitDefinitionByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("UnitDefinition could not be loaded after insert.");
        }

        public async Task<bool> UpdateUnitDefinitionAsync(Guid unitDefinitionId, UpdateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[UnitDefinitions]
SET Code = @Code, Name = @Name, Description = @Description,
    IsActive = @IsActive, SortOrder = @SortOrder, UpdatedAt = @Now
WHERE Id = @UnitDefinitionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                UnitDefinitionId = unitDefinitionId,
                request.Code,
                request.Name,
                request.Description,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteUnitDefinitionAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[UnitDefinitions]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @UnitDefinitionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { UnitDefinitionId = unitDefinitionId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }
    }
}
