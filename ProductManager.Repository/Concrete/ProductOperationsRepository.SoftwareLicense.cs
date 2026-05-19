using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;
using System.Data;

namespace ProductManager.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        // ─── ProductModule ───────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<ProductModuleDto>> GetProductModulesAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, ModuleCode, Name, Description, AdditionalPrice,
 CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductModules]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductModuleDto>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductModuleDto?> GetProductModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, ModuleCode, Name, Description, AdditionalPrice,
 CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductModules]
WHERE Id = @ModuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductModuleDto>(
            new CommandDefinition(sql, new { ModuleId = moduleId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductModuleDto> CreateProductModuleAsync(CreateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductModules]
 (Id, ProductId, ModuleCode, Name, Description, AdditionalPrice,
 CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ModuleCode, @Name, @Description, @AdditionalPrice,
 @CurrencyCode, @IsOptional, @IsActive, @SortOrder, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.ProductId,
                request.ModuleCode,
                request.Name,
                request.Description,
                request.AdditionalPrice,
                request.CurrencyCode,
                request.IsOptional,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetProductModuleByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("ProductModule could not be loaded after insert.");
        }

        public async Task<bool> UpdateProductModuleAsync(Guid moduleId, UpdateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductModules]
SET ModuleCode = @ModuleCode, Name = @Name, Description = @Description,
 AdditionalPrice = @AdditionalPrice, CurrencyCode = @CurrencyCode,
 IsOptional = @IsOptional, IsActive = @IsActive, SortOrder = @SortOrder,
 UpdatedAt = @Now
WHERE Id = @ModuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                ModuleId = moduleId,
                request.ModuleCode,
                request.Name,
                request.Description,
                request.AdditionalPrice,
                request.CurrencyCode,
                request.IsOptional,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteProductModuleAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductModules]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @ModuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ModuleId = moduleId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        // ─── SoftwarePricingTier ─────────────────────────────────────────────────────

        public async Task<IReadOnlyList<SoftwarePricingTierDto>> GetSoftwarePricingTiersAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT t.Id, t.ProductId, t.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
 t.UnitDefinitionId, u.Name AS UnitDefinitionName, t.MinUnits, t.MaxUnits,
 t.PricePerUnit, t.FlatFee, t.CurrencyCode, t.IsActive, t.CreatedAt, t.UpdatedAt
FROM [Product].[SoftwarePricingTiers] t
LEFT JOIN [Product].[ProductLicenseOfferings] o ON o.Id = t.ProductLicenseOfferingId AND o.IsDeleted = 0
LEFT JOIN [Product].[UnitDefinitions] u ON u.Id = t.UnitDefinitionId AND u.IsDeleted = 0
WHERE t.ProductId = @ProductId AND t.IsDeleted = 0
ORDER BY o.Name, t.MinUnits;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<SoftwarePricingTierDto>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<SoftwarePricingTierDto?> GetSoftwarePricingTierByIdAsync(Guid tierId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT t.Id, t.ProductId, t.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
 t.UnitDefinitionId, u.Name AS UnitDefinitionName, t.MinUnits, t.MaxUnits,
 t.PricePerUnit, t.FlatFee, t.CurrencyCode, t.IsActive, t.CreatedAt, t.UpdatedAt
FROM [Product].[SoftwarePricingTiers] t
LEFT JOIN [Product].[ProductLicenseOfferings] o ON o.Id = t.ProductLicenseOfferingId AND o.IsDeleted = 0
LEFT JOIN [Product].[UnitDefinitions] u ON u.Id = t.UnitDefinitionId AND u.IsDeleted = 0
WHERE t.Id = @TierId AND t.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<SoftwarePricingTierDto>(
            new CommandDefinition(sql, new { TierId = tierId }, cancellationToken: cancellationToken));
        }

        public async Task<SoftwarePricingTierDto> CreateSoftwarePricingTierAsync(CreateSoftwarePricingTierRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[SoftwarePricingTiers]
 (Id, ProductId, ProductLicenseOfferingId, UnitDefinitionId, MinUnits, MaxUnits,
 PricePerUnit, FlatFee, CurrencyCode, IsActive, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ProductLicenseOfferingId, @UnitDefinitionId, @MinUnits, @MaxUnits,
 @PricePerUnit, @FlatFee, @CurrencyCode, @IsActive, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
 await connection.ExecuteAsync(new CommandDefinition(sql, new
 {
 Id = id,
 ProductId = request.ProductId ?? Guid.Empty,
 request.ProductLicenseOfferingId,
 request.UnitDefinitionId,
                request.MinUnits,
                request.MaxUnits,
                request.PricePerUnit,
                request.FlatFee,
                request.CurrencyCode,
                request.IsActive,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetSoftwarePricingTierByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("SoftwarePricingTier could not be loaded after insert.");
        }

        public async Task<bool> UpdateSoftwarePricingTierAsync(Guid tierId, UpdateSoftwarePricingTierRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[SoftwarePricingTiers]
SET ProductLicenseOfferingId = @ProductLicenseOfferingId, UnitDefinitionId = @UnitDefinitionId, MinUnits = @MinUnits,
 MaxUnits = @MaxUnits, PricePerUnit = @PricePerUnit, FlatFee = @FlatFee,
 CurrencyCode = @CurrencyCode, IsActive = @IsActive, UpdatedAt = @Now
WHERE Id = @TierId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                TierId = tierId,
                request.ProductLicenseOfferingId,
                request.UnitDefinitionId,
                request.MinUnits,
                request.MaxUnits,
                request.PricePerUnit,
                request.FlatFee,
                request.CurrencyCode,
                request.IsActive,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteSoftwarePricingTierAsync(Guid tierId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[SoftwarePricingTiers]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @TierId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { TierId = tierId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        // ─── ProductLicenseOffering ──────────────────────────────────────────────────

        public async Task<IReadOnlyList<ProductLicenseOfferingDto>> GetProductLicenseOfferingsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, LicenseModel, Name, Description, BasePrice, CurrencyCode,
 BillingPeriodUnit, BillingPeriodValue, AutoRenew, GracePeriodDays,
 TrialDays, ConvertToOfferingId, MaxSeats, ValidFrom, ValidTo,
 IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductLicenseOfferings]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder, LicenseModel;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductLicenseOfferingDto>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductLicenseOfferingDto?> GetProductLicenseOfferingByIdAsync(Guid offeringId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, LicenseModel, Name, Description, BasePrice, CurrencyCode,
 BillingPeriodUnit, BillingPeriodValue, AutoRenew, GracePeriodDays,
 TrialDays, ConvertToOfferingId, MaxSeats, ValidFrom, ValidTo,
 IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductLicenseOfferings]
WHERE Id = @OfferingId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductLicenseOfferingDto>(
            new CommandDefinition(sql, new { OfferingId = offeringId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductLicenseOfferingDto> CreateProductLicenseOfferingAsync(CreateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductLicenseOfferings]
 (Id, ProductId, LicenseModel, Name, Description, BasePrice, CurrencyCode,
 BillingPeriodUnit, BillingPeriodValue, AutoRenew, GracePeriodDays,
 TrialDays, ConvertToOfferingId, MaxSeats, ValidFrom, ValidTo,
 IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @LicenseModel, @Name, @Description, @BasePrice, @CurrencyCode,
 @BillingPeriodUnit, @BillingPeriodValue, @AutoRenew, @GracePeriodDays,
 @TrialDays, @ConvertToOfferingId, @MaxSeats, @ValidFrom, @ValidTo,
 @IsActive, @SortOrder, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.ProductId,
                request.LicenseModel,
                request.Name,
                request.Description,
                request.BasePrice,
                request.CurrencyCode,
                request.BillingPeriodUnit,
                request.BillingPeriodValue,
                request.AutoRenew,
                request.GracePeriodDays,
                request.TrialDays,
                request.ConvertToOfferingId,
                request.MaxSeats,
                request.ValidFrom,
                request.ValidTo,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetProductLicenseOfferingByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("ProductLicenseOffering could not be loaded after insert.");
        }

        public async Task<bool> UpdateProductLicenseOfferingAsync(Guid offeringId, UpdateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductLicenseOfferings]
SET LicenseModel = @LicenseModel, Name = @Name, Description = @Description,
 BasePrice = @BasePrice, CurrencyCode = @CurrencyCode,
 BillingPeriodUnit = @BillingPeriodUnit, BillingPeriodValue = @BillingPeriodValue,
 AutoRenew = @AutoRenew, GracePeriodDays = @GracePeriodDays,
 TrialDays = @TrialDays, ConvertToOfferingId = @ConvertToOfferingId,
 MaxSeats = @MaxSeats, ValidFrom = @ValidFrom, ValidTo = @ValidTo,
 IsActive = @IsActive, SortOrder = @SortOrder, UpdatedAt = @Now
WHERE Id = @OfferingId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                OfferingId = offeringId,
                request.LicenseModel,
                request.Name,
                request.Description,
                request.BasePrice,
                request.CurrencyCode,
                request.BillingPeriodUnit,
                request.BillingPeriodValue,
                request.AutoRenew,
                request.GracePeriodDays,
                request.TrialDays,
                request.ConvertToOfferingId,
                request.MaxSeats,
                request.ValidFrom,
                request.ValidTo,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteProductLicenseOfferingAsync(Guid offeringId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductLicenseOfferings]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @OfferingId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { OfferingId = offeringId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        // ─── CreateProductFull insert helpers ───────────────────────────────────────

        private static async Task InsertModulesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid productId,
        DateTime now,
        IReadOnlyList<CreateProductModuleRequestDto>? modules,
        CancellationToken cancellationToken)
        {
            if (modules is null || modules.Count == 0) return;

            const string sql = @"
INSERT INTO [Product].[ProductModules]
 (Id, ProductId, ModuleCode, Name, Description, AdditionalPrice,
 CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ModuleCode, @Name, @Description, @AdditionalPrice,
 @CurrencyCode, @IsOptional, @IsActive, @SortOrder, @Now, 0);";

            var parameters = modules.Select(m => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                m.ModuleCode,
                m.Name,
                m.Description,
                m.AdditionalPrice,
                m.CurrencyCode,
                m.IsOptional,
                m.IsActive,
                m.SortOrder,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertSoftwarePricingTiersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid productId,
        DateTime now,
        IReadOnlyList<CreateSoftwarePricingTierRequestDto>? tiers,
        IReadOnlyDictionary<string, Guid>? tempIdMap,
        CancellationToken cancellationToken)
        {
            if (tiers is null || tiers.Count == 0) return;

            const string sql = @"
INSERT INTO [Product].[SoftwarePricingTiers]
 (Id, ProductId, ProductLicenseOfferingId, UnitDefinitionId, MinUnits, MaxUnits,
 PricePerUnit, FlatFee, CurrencyCode, IsActive, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ProductLicenseOfferingId, @UnitDefinitionId, @MinUnits, @MaxUnits,
 @PricePerUnit, @FlatFee, @CurrencyCode, @IsActive, @Now, 0);";

 var parameters = tiers.Select(t =>
 {
 // TempId varsa map'ten çözümle, yoksa direkt ProductLicenseOfferingId'yi kullan
 Guid? resolvedOfferingId = t.ProductLicenseOfferingId;
 if (t.LicenseOfferingTempId is not null && tempIdMap is not null)
 {
 if (tempIdMap.TryGetValue(t.LicenseOfferingTempId, out var mappedId))
 resolvedOfferingId = mappedId;
 }

 return new
 {
 Id = Guid.NewGuid(),
 ProductId = productId,
 ProductLicenseOfferingId = resolvedOfferingId,
                    UnitDefinitionId = t.UnitDefinitionId,
                    t.MinUnits,
                    t.MaxUnits,
                    t.PricePerUnit,
                    t.FlatFee,
                    t.CurrencyCode,
                    t.IsActive,
                    Now = now
                };
            }).ToList();

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task<IReadOnlyDictionary<string, Guid>> InsertLicenseOfferingsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid productId,
        DateTime now,
        IReadOnlyList<CreateProductLicenseOfferingRequestDto>? offerings,
        CancellationToken cancellationToken)
        {
            if (offerings is null || offerings.Count == 0) return new Dictionary<string, Guid>();

            const string sql = @"
INSERT INTO [Product].[ProductLicenseOfferings]
 (Id, ProductId, LicenseModel, Name, Description, BasePrice, CurrencyCode,
 BillingPeriodUnit, BillingPeriodValue, AutoRenew, GracePeriodDays,
 TrialDays, ConvertToOfferingId, MaxSeats, ValidFrom, ValidTo,
 IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @LicenseModel, @Name, @Description, @BasePrice, @CurrencyCode,
 @BillingPeriodUnit, @BillingPeriodValue, @AutoRenew, @GracePeriodDays,
 @TrialDays, @ConvertToOfferingId, @MaxSeats, @ValidFrom, @ValidTo,
 @IsActive, @SortOrder, @Now, 0);";

            // Build the params and track tempId → realId
            var tempIdMap = new Dictionary<string, Guid>();
            var parameters = offerings.Select(o =>
            {
                var realId = Guid.NewGuid();
                if (!string.IsNullOrEmpty(o.TempId))
                    tempIdMap[o.TempId] = realId;
                return new
                {
                    Id = realId,
                    ProductId = productId,
                    o.LicenseModel,
                    o.Name,
                    o.Description,
                    o.BasePrice,
                    o.CurrencyCode,
                    o.BillingPeriodUnit,
                    o.BillingPeriodValue,
                    o.AutoRenew,
                    o.GracePeriodDays,
                    o.TrialDays,
                    o.ConvertToOfferingId,
                    o.MaxSeats,
                    o.ValidFrom,
                    o.ValidTo,
                    o.IsActive,
                    o.SortOrder,
                    Now = now
                };
            }).ToList();

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
            return tempIdMap;
        }
    }
}
