using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            IReadOnlyDictionary<string, Guid>? productUnitTempIdMap,
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

            var assignments = new List<(Guid PricingRuleId, IReadOnlyList<Guid> ProductUnitIds)>();
            var parameters = rules.Select(rule =>
            {
                var id = Guid.NewGuid();
                var productUnitIds = ResolveProductUnitIds(
                    rule.ProductUnitIds,
                    rule.ProductUnitTempIds,
                    productUnitTempIdMap);
                assignments.Add((id, productUnitIds));
                return new
                {
                    Id = id,
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
                };
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
            foreach (var assignment in assignments)
            {
                await InsertPricingRuleUnitAssignmentsAsync(
                    connection,
                    transaction,
                    assignment.PricingRuleId,
                    assignment.ProductUnitIds,
                    now,
                    cancellationToken);
            }
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
SELECT r.Id, r.ProductId, r.Code, r.Name, r.Description, r.PriceAdjustmentJson, r.ConditionsJson,
       r.Priority, r.IsActive, r.ValidFrom, r.ValidTo, r.SalesChannel, r.CustomerGroupCode,
       r.ProductVariantId, r.ProductLicenseOfferingId,
       r.CreatedAt, r.UpdatedAt
FROM [Product].[ProductPricingRules] r
WHERE r.ProductId = @ProductId AND r.IsDeleted = 0
ORDER BY r.Priority, r.CreatedAt;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductPricingRuleDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
            var rules = items.Select(NormalizePricingRuleDto).ToList();
            var unitsByRuleId = await LoadPricingRuleUnitsAsync(connection, rules.Select(r => r.Id), cancellationToken);
            return AttachProductUnits(rules, unitsByRuleId);
        }

        public async Task<ProductPricingRuleDto?> GetPricingRuleByIdAsync(
            Guid pricingRuleId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT r.Id, r.ProductId, r.Code, r.Name, r.Description, r.PriceAdjustmentJson, r.ConditionsJson,
       r.Priority, r.IsActive, r.ValidFrom, r.ValidTo, r.SalesChannel, r.CustomerGroupCode,
       r.ProductVariantId, r.ProductLicenseOfferingId,
       r.CreatedAt, r.UpdatedAt
FROM [Product].[ProductPricingRules] r
WHERE r.Id = @PricingRuleId AND r.IsDeleted = 0;";

            using var connection = CreateConnection();
            var pricingRule = await connection.QuerySingleOrDefaultAsync<ProductPricingRuleDto>(
                new CommandDefinition(sql, new { PricingRuleId = pricingRuleId }, cancellationToken: cancellationToken));
            if (pricingRule is null)
            {
                return null;
            }

            var normalizedRule = NormalizePricingRuleDto(pricingRule);
            var unitsByRuleId = await LoadPricingRuleUnitsAsync(connection, [normalizedRule.Id], cancellationToken);
            return AttachProductUnits([normalizedRule], unitsByRuleId).First();
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
            var now = DateTime.UtcNow;
            var priceAdjustmentJson = ResolvePriceAdjustmentJson(request.PriceAdjustmentJson, request.PriceAdjustment);
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
                Now = now
            }, transaction, cancellationToken: cancellationToken));

            await InsertPricingRuleUnitAssignmentsAsync(connection, transaction, id, productUnitIds, now, cancellationToken);

            transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

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
            var now = DateTime.UtcNow;
            var productUnitIds = ResolveProductUnitIds(
                request.ProductUnitIds,
                null,
                null);
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
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
                Now = now
            }, transaction, cancellationToken: cancellationToken));
            if (rows == 0)
            {
                transaction.Rollback();
                return false;
            }

            await DeletePricingRuleUnitAssignmentsAsync(connection, transaction, pricingRuleId, cancellationToken);
            await InsertPricingRuleUnitAssignmentsAsync(connection, transaction, pricingRuleId, productUnitIds, now, cancellationToken);
            transaction.Commit();
            return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static string ResolvePriceAdjustmentJson(string? priceAdjustmentJson, JsonElement? priceAdjustment)
        {
            if (!string.IsNullOrWhiteSpace(priceAdjustmentJson))
            {
                return NormalizePriceAdjustmentOperation(priceAdjustmentJson);
            }

            if (priceAdjustment.HasValue && priceAdjustment.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                return NormalizePriceAdjustmentOperation(priceAdjustment.Value.GetRawText());
            }

            throw new ValidationException("priceAdjustment", "priceAdjustment veya priceAdjustmentJson alanı zorunludur.");
        }

        private static ProductPricingRuleDto NormalizePricingRuleDto(ProductPricingRuleDto pricingRule)
            => string.IsNullOrWhiteSpace(pricingRule.PriceAdjustmentJson)
                ? pricingRule
                : pricingRule with { PriceAdjustmentJson = NormalizePriceAdjustmentOperation(pricingRule.PriceAdjustmentJson) };

        private static string NormalizePriceAdjustmentOperation(string priceAdjustmentJson)
        {
            try
            {
                var node = JsonNode.Parse(priceAdjustmentJson);
                if (node is not JsonObject adjustment)
                {
                    return priceAdjustmentJson;
                }

                var operationPropertyName = FindPropertyName(adjustment, "operation");
                var directionPropertyName = FindPropertyName(adjustment, "direction");
                var operation = NormalizeOperation(ReadJsonString(adjustment, operationPropertyName))
                    ?? NormalizeOperation(ReadJsonString(adjustment, directionPropertyName))
                    ?? "add";

                if (operationPropertyName is not null && operationPropertyName != "operation")
                {
                    adjustment.Remove(operationPropertyName);
                }

                adjustment["operation"] = operation;
                return adjustment.ToJsonString();
            }
            catch (JsonException)
            {
                return priceAdjustmentJson;
            }
        }

        private static string? FindPropertyName(JsonObject jsonObject, string propertyName)
            => jsonObject
                .Select(property => property.Key)
                .FirstOrDefault(key => string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase));

        private static string? ReadJsonString(JsonObject jsonObject, string? propertyName)
        {
            if (propertyName is null || !jsonObject.TryGetPropertyValue(propertyName, out var value) || value is null)
            {
                return null;
            }

            return value.GetValueKind() == JsonValueKind.String
                ? value.GetValue<string>()
                : value.ToJsonString();
        }

        private static string? NormalizeOperation(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return string.Equals(value, "subtract", StringComparison.OrdinalIgnoreCase)
                ? "subtract"
                : "add";
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
