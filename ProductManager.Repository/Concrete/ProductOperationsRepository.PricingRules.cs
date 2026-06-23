using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;
using System.Data;
using System.Text.Json;

namespace ProductManager.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        private static async Task InsertPricingRulesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductPricingRuleRequestDto>? rules,
            IReadOnlyDictionary<string, Guid>? licenseOfferingTempIdMap,
            CancellationToken cancellationToken)
        {
            if (rules is null || rules.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductPricingRules]
(
    Id, ProductId, Code, Name, Description, PriceAdjustmentJson, ConditionsJson,
    Priority, IsActive, ValidFrom, ValidTo, SalesChannel, CustomerGroupCode,
    ProductVariantId, ProductLicenseOfferingId, CreatedAt, IsDeleted
)
VALUES
(
    @Id, @ProductId, @Code, @Name, @Description, @PriceAdjustmentJson, @ConditionsJson,
    @Priority, @IsActive, @ValidFrom, @ValidTo, @SalesChannel, @CustomerGroupCode,
    @ProductVariantId, @ProductLicenseOfferingId, @Now, 0
);";

            var parameters = rules.Select(rule => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                rule.Code,
                rule.Name,
                rule.Description,
                PriceAdjustmentJson = ResolvePriceAdjustmentJson(rule.PriceAdjustmentJson, rule.PriceAdjustment),
                rule.ConditionsJson,
                rule.Priority,
                rule.IsActive,
                rule.ValidFrom,
                rule.ValidTo,
                rule.SalesChannel,
                rule.CustomerGroupCode,
                rule.ProductVariantId,
                ProductLicenseOfferingId = ResolveLicenseOfferingId(
                    rule.ProductLicenseOfferingId,
                    rule.LicenseOfferingTempId,
                    licenseOfferingTempIdMap),
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static Guid? ResolveLicenseOfferingId(
            Guid? productLicenseOfferingId,
            string? licenseOfferingTempId,
            IReadOnlyDictionary<string, Guid>? licenseOfferingTempIdMap)
        {
            if (productLicenseOfferingId.HasValue && productLicenseOfferingId.Value != Guid.Empty)
            {
                return productLicenseOfferingId.Value;
            }

            if (!string.IsNullOrWhiteSpace(licenseOfferingTempId)
                && licenseOfferingTempIdMap is not null
                && licenseOfferingTempIdMap.TryGetValue(licenseOfferingTempId, out var resolvedId))
            {
                return resolvedId;
            }

            return null;
        }

        public async Task<IReadOnlyList<ProductPricingRuleDto>> GetProductPricingRulesAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, Code, Name, Description, PriceAdjustmentJson, ConditionsJson,
       Priority, IsActive, ValidFrom, ValidTo, SalesChannel, CustomerGroupCode,
       ProductVariantId, ProductLicenseOfferingId, CreatedAt, UpdatedAt
FROM [Product].[ProductPricingRules]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY Priority, CreatedAt;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductPricingRuleDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<ProductPricingRuleDto?> GetPricingRuleByIdAsync(
            Guid pricingRuleId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT Id, ProductId, Code, Name, Description, PriceAdjustmentJson, ConditionsJson,
       Priority, IsActive, ValidFrom, ValidTo, SalesChannel, CustomerGroupCode,
       ProductVariantId, ProductLicenseOfferingId, CreatedAt, UpdatedAt
FROM [Product].[ProductPricingRules]
WHERE Id = @PricingRuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductPricingRuleDto>(
                new CommandDefinition(sql, new { PricingRuleId = pricingRuleId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductPricingRuleDto> CreatePricingRuleAsync(
            CreateProductPricingRuleRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductPricingRules]
(
    Id, ProductId, Code, Name, Description, PriceAdjustmentJson, ConditionsJson,
    Priority, IsActive, ValidFrom, ValidTo, SalesChannel, CustomerGroupCode,
    ProductVariantId, ProductLicenseOfferingId, CreatedAt, IsDeleted
)
VALUES
(
    @Id, @ProductId, @Code, @Name, @Description, @PriceAdjustmentJson, @ConditionsJson,
    @Priority, @IsActive, @ValidFrom, @ValidTo, @SalesChannel, @CustomerGroupCode,
    @ProductVariantId, @ProductLicenseOfferingId, @Now, 0
);";

            var id = Guid.NewGuid();
            var priceAdjustmentJson = ResolvePriceAdjustmentJson(request.PriceAdjustmentJson, request.PriceAdjustment);
            using var connection = CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                request.ProductId,
                request.Code,
                request.Name,
                request.Description,
                PriceAdjustmentJson = priceAdjustmentJson,
                request.ConditionsJson,
                request.Priority,
                request.IsActive,
                request.ValidFrom,
                request.ValidTo,
                request.SalesChannel,
                request.CustomerGroupCode,
                request.ProductVariantId,
                request.ProductLicenseOfferingId,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));

            return await GetPricingRuleByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("ProductPricingRule could not be loaded after insert.");
        }

        public async Task<bool> UpdatePricingRuleAsync(
            Guid pricingRuleId,
            UpdateProductPricingRuleRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPricingRules]
SET Code = @Code,
    Name = @Name,
    Description = @Description,
    PriceAdjustmentJson = @PriceAdjustmentJson,
    ConditionsJson = @ConditionsJson,
    Priority = @Priority,
    IsActive = @IsActive,
    ValidFrom = @ValidFrom,
    ValidTo = @ValidTo,
    SalesChannel = @SalesChannel,
    CustomerGroupCode = @CustomerGroupCode,
    ProductVariantId = @ProductVariantId,
    ProductLicenseOfferingId = @ProductLicenseOfferingId,
    UpdatedAt = @Now
WHERE Id = @PricingRuleId AND IsDeleted = 0;";

            var priceAdjustmentJson = ResolvePriceAdjustmentJson(request.PriceAdjustmentJson, request.PriceAdjustment);
            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PricingRuleId = pricingRuleId,
                request.Code,
                request.Name,
                request.Description,
                PriceAdjustmentJson = priceAdjustmentJson,
                request.ConditionsJson,
                request.Priority,
                request.IsActive,
                request.ValidFrom,
                request.ValidTo,
                request.SalesChannel,
                request.CustomerGroupCode,
                request.ProductVariantId,
                request.ProductLicenseOfferingId,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        private static string ResolvePriceAdjustmentJson(string? priceAdjustmentJson, JsonElement? priceAdjustment)
        {
            if (!string.IsNullOrWhiteSpace(priceAdjustmentJson))
            {
                return priceAdjustmentJson;
            }

            if (priceAdjustment.HasValue && priceAdjustment.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                return priceAdjustment.Value.GetRawText();
            }

            throw new ValidationException("priceAdjustment", "priceAdjustment veya priceAdjustmentJson alanı zorunludur.");
        }

        public async Task<bool> DeletePricingRuleAsync(
            Guid pricingRuleId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPricingRules]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @PricingRuleId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PricingRuleId = pricingRuleId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }
    }
}
