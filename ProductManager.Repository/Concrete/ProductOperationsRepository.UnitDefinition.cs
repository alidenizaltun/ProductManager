using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;
using System.Data;

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

            var id = await InsertWithGeneratedCodeAsync(
                request.Code,
                UnitDefinitionCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var unitDefinitionId = Guid.NewGuid();
                    await connection.ExecuteAsync(new CommandDefinition(sql, new
                    {
                        Id = unitDefinitionId,
                        Code = code,
                        request.Name,
                        request.Description,
                        request.IsActive,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    }, transaction, cancellationToken: ct));
                    return unitDefinitionId;
                },
                cancellationToken);

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

        public async Task<IReadOnlyList<ProductUnitDto>> GetProductUnitsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT pu.Id, pu.ProductId, pu.UnitDefinitionId,
       ud.Code AS UnitDefinitionCode, ud.Name AS UnitDefinitionName,
       pu.Code, pu.Name, pu.Description, pu.Role, pu.IsDefault, pu.IsActive,
       pu.SortOrder, pu.CreatedAt, pu.UpdatedAt
FROM [Product].[ProductUnits] pu
JOIN [Product].[UnitDefinitions] ud ON ud.Id = pu.UnitDefinitionId AND ud.IsDeleted = 0
WHERE pu.ProductId = @ProductId AND pu.IsDeleted = 0
ORDER BY pu.SortOrder, pu.Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductUnitDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductUnitDto?> GetProductUnitByIdAsync(Guid productUnitId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT pu.Id, pu.ProductId, pu.UnitDefinitionId,
       ud.Code AS UnitDefinitionCode, ud.Name AS UnitDefinitionName,
       pu.Code, pu.Name, pu.Description, pu.Role, pu.IsDefault, pu.IsActive,
       pu.SortOrder, pu.CreatedAt, pu.UpdatedAt
FROM [Product].[ProductUnits] pu
JOIN [Product].[UnitDefinitions] ud ON ud.Id = pu.UnitDefinitionId AND ud.IsDeleted = 0
WHERE pu.Id = @ProductUnitId AND pu.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductUnitDto>(
                new CommandDefinition(sql, new { ProductUnitId = productUnitId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductUnitDto> CreateProductUnitAsync(CreateProductUnitRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductUnits]
    (Id, ProductId, UnitDefinitionId, Code, Name, Description, Role, IsDefault, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @ProductId, @UnitDefinitionId, @Code, @Name, @Description, @Role, @IsDefault, @IsActive, @SortOrder, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.ProductId,
                request.UnitDefinitionId,
                request.Code,
                request.Name,
                request.Description,
                request.Role,
                request.IsDefault,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetProductUnitByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("ProductUnit could not be loaded after insert.");
        }

        public async Task<bool> UpdateProductUnitAsync(Guid productUnitId, UpdateProductUnitRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductUnits]
SET UnitDefinitionId = @UnitDefinitionId,
    Code = @Code,
    Name = @Name,
    Description = @Description,
    Role = @Role,
    IsDefault = @IsDefault,
    IsActive = @IsActive,
    SortOrder = @SortOrder,
    UpdatedAt = @Now
WHERE Id = @ProductUnitId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                ProductUnitId = productUnitId,
                request.UnitDefinitionId,
                request.Code,
                request.Name,
                request.Description,
                request.Role,
                request.IsDefault,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteProductUnitAsync(Guid productUnitId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductUnits]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @ProductUnitId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductUnitId = productUnitId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        private static async Task<IReadOnlyDictionary<string, Guid>> InsertProductUnitsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductUnitRequestDto>? productUnits,
            CancellationToken cancellationToken)
        {
            if (productUnits is null || productUnits.Count == 0) return new Dictionary<string, Guid>();

            const string sql = @"
INSERT INTO [Product].[ProductUnits]
    (Id, ProductId, UnitDefinitionId, Code, Name, Description, Role, IsDefault, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @ProductId, @UnitDefinitionId, @Code, @Name, @Description, @Role, @IsDefault, @IsActive, @SortOrder, @Now, 0);";

            var tempIdMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var parameters = productUnits.Select(unit =>
            {
                var id = unit.Id is { } existingId && existingId != Guid.Empty
                    ? existingId
                    : Guid.NewGuid();
                if (!string.IsNullOrWhiteSpace(unit.TempId))
                    tempIdMap[unit.TempId] = id;

                return new
                {
                    Id = id,
                    ProductId = productId,
                    unit.UnitDefinitionId,
                    unit.Code,
                    unit.Name,
                    unit.Description,
                    unit.Role,
                    unit.IsDefault,
                    unit.IsActive,
                    unit.SortOrder,
                    Now = now
                };
            }).ToList();

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
            return tempIdMap;
        }
    }
}
