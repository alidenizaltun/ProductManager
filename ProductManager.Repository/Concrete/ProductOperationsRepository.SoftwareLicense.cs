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
SELECT Id, ProductId, ModuleCode, Name, Description,
 IsOptional, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductModules]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder, Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductModuleDto>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return await AttachModuleOfferingPricesAsync(connection, items.AsList(), cancellationToken);
        }

        public async Task<ProductModuleDto?> GetProductModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, ModuleCode, Name, Description,
 IsOptional, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductModules]
WHERE Id = @ModuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var item = await connection.QuerySingleOrDefaultAsync<ProductModuleDto>(
            new CommandDefinition(sql, new { ModuleId = moduleId }, cancellationToken: cancellationToken));
            if (item is null)
            {
                return null;
            }

            var items = await AttachModuleOfferingPricesAsync(connection, [item], cancellationToken);
            return items.First();
        }

        public async Task<ProductModuleDto> CreateProductModuleAsync(CreateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductModules]
 (Id, ProductId, ModuleCode, Name, Description,
 IsOptional, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ModuleCode, @Name, @Description,
 @IsOptional, @IsActive, @SortOrder, @Now, 0);";

            const string priceSql = @"
INSERT INTO [Product].[ProductModuleOfferingPrices]
 (Id, ProductModuleId, ProductLicenseOfferingId, Price, CurrencyCode, IsActive, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductModuleId, @ProductLicenseOfferingId, @Price, @CurrencyCode, @IsActive, @Now, 0);";

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(new CommandDefinition(sql, new
                {
                    Id = id,
                    request.ProductId,
                    request.ModuleCode,
                    request.Name,
                    request.Description,
                    request.IsOptional,
                    request.IsActive,
                    request.SortOrder,
                    Now = now
                }, transaction, cancellationToken: cancellationToken));

                if (request.OfferingPrices is { Count: > 0 })
                {
                    var priceParameters = request.OfferingPrices
                        .Where(op => op.ProductLicenseOfferingId is { } offeringId && offeringId != Guid.Empty)
                        .Select(op => new
                        {
                            Id = Guid.NewGuid(),
                            ProductModuleId = id,
                            ProductLicenseOfferingId = op.ProductLicenseOfferingId!.Value,
                            op.Price,
                            op.CurrencyCode,
                            op.IsActive,
                            Now = now
                        })
                        .ToList();

                    if (priceParameters.Count > 0)
                    {
                        await connection.ExecuteAsync(new CommandDefinition(priceSql, priceParameters, transaction, cancellationToken: cancellationToken));
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return await GetProductModuleByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("ProductModule could not be loaded after insert.");
        }

        public async Task<bool> UpdateProductModuleAsync(Guid moduleId, UpdateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductModules]
SET ModuleCode = @ModuleCode, Name = @Name, Description = @Description,
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

        private static async Task<IReadOnlyList<ProductModuleDto>> AttachModuleOfferingPricesAsync(
        IDbConnection connection,
        IReadOnlyList<ProductModuleDto> modules,
        CancellationToken cancellationToken)
        {
            if (modules.Count == 0)
            {
                return modules;
            }

            const string sql = @"
SELECT p.Id, p.ProductModuleId, m.ModuleCode, m.Name AS ModuleName,
 p.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
 p.Price, p.CurrencyCode, p.IsActive, p.CreatedAt, p.UpdatedAt
FROM [Product].[ProductModuleOfferingPrices] p
JOIN [Product].[ProductModules] m ON m.Id = p.ProductModuleId AND m.IsDeleted = 0
JOIN [Product].[ProductLicenseOfferings] o ON o.Id = p.ProductLicenseOfferingId AND o.IsDeleted = 0
WHERE p.ProductModuleId IN @ModuleIds AND p.IsDeleted = 0
ORDER BY o.Name;";

            var prices = (await connection.QueryAsync<ProductModuleOfferingPriceDto>(
                new CommandDefinition(sql, new { ModuleIds = modules.Select(m => m.Id).ToArray() }, cancellationToken: cancellationToken)))
                .AsList();
            if (prices.Count == 0)
            {
                return modules;
            }

            var pricesByModule = prices
                .GroupBy(p => p.ProductModuleId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<ProductModuleOfferingPriceDto>)g.ToList());

            return modules
                .Select(m => m with
                {
                    OfferingPrices = pricesByModule.TryGetValue(m.Id, out var offeringPrices) ? offeringPrices : []
                })
                .ToList();
        }

        // ─── ProductModuleOfferingPrice ──────────────────────────────────────────────

        public async Task<IReadOnlyList<ProductModuleOfferingPriceDto>> GetModuleOfferingPricesAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT p.Id, p.ProductModuleId, m.ModuleCode, m.Name AS ModuleName,
 p.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
 p.Price, p.CurrencyCode, p.IsActive, p.CreatedAt, p.UpdatedAt
FROM [Product].[ProductModuleOfferingPrices] p
JOIN [Product].[ProductModules] m ON m.Id = p.ProductModuleId AND m.IsDeleted = 0
JOIN [Product].[ProductLicenseOfferings] o ON o.Id = p.ProductLicenseOfferingId AND o.IsDeleted = 0
WHERE p.ProductModuleId = @ModuleId AND p.IsDeleted = 0
ORDER BY o.Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductModuleOfferingPriceDto>(
            new CommandDefinition(sql, new { ModuleId = moduleId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductModuleOfferingPriceDto?> GetModuleOfferingPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT p.Id, p.ProductModuleId, m.ModuleCode, m.Name AS ModuleName,
 p.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
 p.Price, p.CurrencyCode, p.IsActive, p.CreatedAt, p.UpdatedAt
FROM [Product].[ProductModuleOfferingPrices] p
JOIN [Product].[ProductModules] m ON m.Id = p.ProductModuleId AND m.IsDeleted = 0
JOIN [Product].[ProductLicenseOfferings] o ON o.Id = p.ProductLicenseOfferingId AND o.IsDeleted = 0
WHERE p.Id = @PriceId AND p.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductModuleOfferingPriceDto>(
            new CommandDefinition(sql, new { PriceId = priceId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductModuleOfferingPriceDto> CreateModuleOfferingPriceAsync(CreateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductModuleOfferingPrices]
 (Id, ProductModuleId, ProductLicenseOfferingId, Price, CurrencyCode, IsActive, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductModuleId, @ProductLicenseOfferingId, @Price, @CurrencyCode, @IsActive, @Now, 0);";

            var id = Guid.NewGuid();
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.ProductModuleId,
                request.ProductLicenseOfferingId,
                request.Price,
                request.CurrencyCode,
                request.IsActive,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetModuleOfferingPriceByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("ProductModuleOfferingPrice could not be loaded after insert.");
        }

        public async Task<bool> UpdateModuleOfferingPriceAsync(Guid priceId, UpdateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductModuleOfferingPrices]
SET Price = @Price, CurrencyCode = @CurrencyCode, IsActive = @IsActive, UpdatedAt = @Now
WHERE Id = @PriceId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PriceId = priceId,
                request.Price,
                request.CurrencyCode,
                request.IsActive,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeleteModuleOfferingPriceAsync(Guid priceId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductModuleOfferingPrices]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @PriceId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { PriceId = priceId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        // ─── ProductLicenseOffering ──────────────────────────────────────────────────

        public async Task<IReadOnlyList<ProductLicenseOfferingDto>> GetProductLicenseOfferingsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT o.Id, o.ProductId,
 o.LicenseModel, o.Name, o.Description, o.BasePrice, o.CurrencyCode,
 o.BillingPeriodUnit, o.BillingPeriodValue, o.AutoRenew, o.GracePeriodDays,
 o.TrialDays, o.ConvertToOfferingId, cto.Name AS ConvertToOfferingName, o.MaxSeats, o.ValidFrom, o.ValidTo,
 o.IsActive, o.SortOrder, o.CreatedAt, o.UpdatedAt
FROM [Product].[ProductLicenseOfferings] o
LEFT JOIN [Product].[ProductLicenseOfferings] cto ON cto.Id = o.ConvertToOfferingId AND cto.IsDeleted = 0
WHERE o.ProductId = @ProductId AND o.IsDeleted = 0
ORDER BY o.SortOrder, o.LicenseModel;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductLicenseOfferingDto>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            var offerings = items.AsList();
            var unitsByOfferingId = await LoadLicenseOfferingUnitsAsync(connection, offerings.Select(o => o.Id), cancellationToken);
            return AttachProductUnits(offerings, unitsByOfferingId);
        }

        public async Task<ProductLicenseOfferingDto?> GetProductLicenseOfferingByIdAsync(Guid offeringId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT o.Id, o.ProductId,
 o.LicenseModel, o.Name, o.Description, o.BasePrice, o.CurrencyCode,
 o.BillingPeriodUnit, o.BillingPeriodValue, o.AutoRenew, o.GracePeriodDays,
 o.TrialDays, o.ConvertToOfferingId, cto.Name AS ConvertToOfferingName, o.MaxSeats, o.ValidFrom, o.ValidTo,
 o.IsActive, o.SortOrder, o.CreatedAt, o.UpdatedAt
FROM [Product].[ProductLicenseOfferings] o
LEFT JOIN [Product].[ProductLicenseOfferings] cto ON cto.Id = o.ConvertToOfferingId AND cto.IsDeleted = 0
WHERE o.Id = @OfferingId AND o.IsDeleted = 0;";

            using var connection = CreateConnection();
            var offering = await connection.QuerySingleOrDefaultAsync<ProductLicenseOfferingDto>(
            new CommandDefinition(sql, new { OfferingId = offeringId }, cancellationToken: cancellationToken));
            if (offering is null)
            {
                return null;
            }

            var unitsByOfferingId = await LoadLicenseOfferingUnitsAsync(connection, [offering.Id], cancellationToken);
            return AttachProductUnits([offering], unitsByOfferingId).First();
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
            var now = DateTime.UtcNow;
            var productUnitIds = ResolveProductUnitIds(
                request.ProductUnitIds,
                request.ProductUnitTempIds,
                null);
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
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
                Now = now
            }, transaction, cancellationToken: cancellationToken));

            await InsertLicenseOfferingUnitAssignmentsAsync(connection, transaction, id, productUnitIds, now, cancellationToken);

            transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

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

            var now = DateTime.UtcNow;
            var productUnitIds = request.ProductUnitIds is null
                ? null
                : ResolveProductUnitIds(request.ProductUnitIds, null, null);
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
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
                Now = now
            }, transaction, cancellationToken: cancellationToken));
            if (rows == 0)
            {
                transaction.Rollback();
                return false;
            }

            if (productUnitIds is not null)
            {
                await DeleteLicenseOfferingUnitAssignmentsAsync(connection, transaction, offeringId, cancellationToken);
                await InsertLicenseOfferingUnitAssignmentsAsync(connection, transaction, offeringId, productUnitIds, now, cancellationToken);
            }
            transaction.Commit();
            return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        private static async Task<IReadOnlyDictionary<string, Guid>> InsertModulesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid productId,
        DateTime now,
        IReadOnlyList<CreateProductModuleRequestDto>? modules,
        CancellationToken cancellationToken)
        {
            if (modules is null || modules.Count == 0) return new Dictionary<string, Guid>();

            const string sql = @"
INSERT INTO [Product].[ProductModules]
 (Id, ProductId, ModuleCode, Name, Description,
 IsOptional, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductId, @ModuleCode, @Name, @Description,
 @IsOptional, @IsActive, @SortOrder, @Now, 0);";

            var moduleCodeMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var parameters = modules.Select(m =>
            {
                var id = Guid.NewGuid();
                moduleCodeMap[m.ModuleCode] = id;
                return new
                {
                    Id = id,
                    ProductId = productId,
                    m.ModuleCode,
                    m.Name,
                    m.Description,
                    m.IsOptional,
                    m.IsActive,
                    m.SortOrder,
                    Now = now
                };
            }).ToList();

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
            return moduleCodeMap;
        }

        private static async Task InsertModuleOfferingPricesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        DateTime now,
        IReadOnlyList<CreateProductModuleRequestDto>? modules,
        IReadOnlyDictionary<string, Guid> moduleCodeMap,
        IReadOnlyDictionary<string, Guid> tempIdMap,
        CancellationToken cancellationToken)
        {
            if (modules is null || modules.Count == 0) return;

            const string sql = @"
INSERT INTO [Product].[ProductModuleOfferingPrices]
 (Id, ProductModuleId, ProductLicenseOfferingId, Price, CurrencyCode, IsActive, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductModuleId, @ProductLicenseOfferingId, @Price, @CurrencyCode, @IsActive, @Now, 0);";

            var parameters = new List<object>();
            foreach (var module in modules)
            {
                if (module.OfferingPrices is null || module.OfferingPrices.Count == 0) continue;
                if (!moduleCodeMap.TryGetValue(module.ModuleCode, out var moduleId)) continue;

                foreach (var op in module.OfferingPrices)
                {
                    Guid? resolvedOfferingId = op.ProductLicenseOfferingId;
                    if (op.LicenseOfferingTempId is not null && tempIdMap.TryGetValue(op.LicenseOfferingTempId, out var mappedId))
                        resolvedOfferingId = mappedId;

                    if (resolvedOfferingId is null || resolvedOfferingId == Guid.Empty) continue;

                    parameters.Add(new
                    {
                        Id = Guid.NewGuid(),
                        ProductModuleId = moduleId,
                        ProductLicenseOfferingId = resolvedOfferingId.Value,
                        op.Price,
                        op.CurrencyCode,
                        op.IsActive,
                        Now = now
                    });
                }
            }

            if (parameters.Count == 0) return;
            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task<IReadOnlyDictionary<string, Guid>> InsertLicenseOfferingsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid productId,
        DateTime now,
        IReadOnlyList<CreateProductLicenseOfferingRequestDto>? offerings,
        IReadOnlyDictionary<string, Guid>? productUnitTempIdMap,
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
            var assignments = new List<(Guid OfferingId, IReadOnlyList<Guid> ProductUnitIds)>();
            var parameters = offerings.Select(o =>
            {
                var realId = o.Id is { } existingId && existingId != Guid.Empty
     ? existingId
      : Guid.NewGuid();
                if (!string.IsNullOrEmpty(o.TempId))
                    tempIdMap[o.TempId] = realId;
                var productUnitIds = ResolveProductUnitIds(
                    o.ProductUnitIds,
                    o.ProductUnitTempIds,
                    productUnitTempIdMap);
                assignments.Add((realId, productUnitIds));
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
            foreach (var assignment in assignments)
            {
                await InsertLicenseOfferingUnitAssignmentsAsync(
                    connection,
                    transaction,
                    assignment.OfferingId,
                    assignment.ProductUnitIds,
                    now,
                    cancellationToken);
            }

            return tempIdMap;
        }
    }
}
