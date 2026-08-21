using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using System.Data;

namespace ProductManagement.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var sql = @"
SELECT Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[Regions]
WHERE IsDeleted = 0" + (includeInactive ? "" : " AND IsActive = 1") + @"
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<RegionDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetRegionLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var sql = @"
SELECT Id, Name
FROM [Product].[Regions]
WHERE IsDeleted = 0" + (includeInactive ? "" : " AND IsActive = 1") + @"
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<RegionDto?> GetRegionByIdAsync(Guid regionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[Regions]
WHERE Id = @RegionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<RegionDto>(
                new CommandDefinition(sql, new { RegionId = regionId }, cancellationToken: cancellationToken));
        }

        public async Task<RegionDto> CreateRegionAsync(CreateRegionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[Regions]
    (Id, Code, Name, Description, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @Code, @Name, @Description, @IsActive, @SortOrder, @Now, 0);";

            var id = await InsertWithGeneratedCodeAsync(
                request.Code,
                RegionCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var regionId = Guid.NewGuid();
                    await connection.ExecuteAsync(new CommandDefinition(sql, new
                    {
                        Id = regionId,
                        Code = code,
                        request.Name,
                        request.Description,
                        request.IsActive,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    }, transaction, cancellationToken: ct));
                    return regionId;
                },
                cancellationToken);

            return await GetRegionByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Region could not be loaded after insert.");
        }

        public async Task<bool> UpdateRegionAsync(Guid regionId, UpdateRegionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[Regions]
SET Code = @Code, Name = @Name, Description = @Description,
    IsActive = @IsActive, SortOrder = @SortOrder, UpdatedAt = @Now
WHERE Id = @RegionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                RegionId = regionId,
                request.Code,
                request.Name,
                request.Description,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteRegionAsync(Guid regionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[Regions]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @RegionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { RegionId = regionId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<IReadOnlyList<ProductRegionDto>> GetProductRegionsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = ProductRegionSelect + @"
WHERE pr.ProductId = @ProductId AND pr.IsDeleted = 0
ORDER BY pr.SortOrder, r.Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductRegionDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductRegionDto?> GetProductRegionByIdAsync(Guid productRegionId, CancellationToken cancellationToken = default)
        {
            const string sql = ProductRegionSelect + @"
WHERE pr.Id = @ProductRegionId AND pr.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductRegionDto>(
                new CommandDefinition(sql, new { ProductRegionId = productRegionId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductRegionDto> CreateProductRegionAsync(CreateProductRegionRequestDto request, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(ProductRegionInsert, new
            {
                Id = id,
                request.ProductId,
                request.RegionId,
                request.CurrencyCode,
                request.TaxRate,
                request.IsDefault,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetProductRegionByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("ProductRegion could not be loaded after insert.");
        }

        public async Task<bool> UpdateProductRegionAsync(Guid productRegionId, UpdateProductRegionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductRegions]
SET RegionId = @RegionId,
    CurrencyCode = @CurrencyCode,
    TaxRate = @TaxRate,
    IsDefault = @IsDefault,
    IsActive = @IsActive,
    SortOrder = @SortOrder,
    UpdatedAt = @Now
WHERE Id = @ProductRegionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                ProductRegionId = productRegionId,
                request.RegionId,
                request.CurrencyCode,
                request.TaxRate,
                request.IsDefault,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteProductRegionAsync(Guid productRegionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductRegions]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @ProductRegionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductRegionId = productRegionId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        private const string ProductRegionSelect = @"
SELECT pr.Id, pr.ProductId, pr.RegionId,
       r.Code AS RegionCode, r.Name AS RegionName,
       pr.CurrencyCode, pr.TaxRate, pr.IsDefault, pr.IsActive,
       pr.SortOrder, pr.CreatedAt, pr.UpdatedAt
FROM [Product].[ProductRegions] pr
JOIN [Product].[Regions] r ON r.Id = pr.RegionId AND r.IsDeleted = 0";

        private const string ProductRegionInsert = @"
INSERT INTO [Product].[ProductRegions]
    (Id, ProductId, RegionId, CurrencyCode, TaxRate, IsDefault, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @ProductId, @RegionId, @CurrencyCode, @TaxRate, @IsDefault, @IsActive, @SortOrder, @Now, 0);";

        /// <summary>
        /// Aynı bölgenin iki kez gönderilmesi unique index'e takılacağı için ilk kayıt
        /// korunur; ürün oluşturma/güncelleme akışının tamamı iptal olmaz.
        /// </summary>
        private static async Task InsertProductRegionsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductRegionRequestDto>? regions,
            CancellationToken cancellationToken)
        {
            if (regions is null || regions.Count == 0) return;

            var parameters = regions
                .Where(region => region.RegionId != Guid.Empty)
                .GroupBy(region => region.RegionId)
                .Select(group => group.First())
                .Select(region => new
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    region.RegionId,
                    region.CurrencyCode,
                    region.TaxRate,
                    region.IsDefault,
                    region.IsActive,
                    region.SortOrder,
                    Now = now
                })
                .ToList();

            if (parameters.Count == 0) return;

            await connection.ExecuteAsync(new CommandDefinition(ProductRegionInsert, parameters, transaction, cancellationToken: cancellationToken));
        }
    }
}
