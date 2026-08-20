using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using System.Data;

namespace ProductManagement.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        private sealed record ProductUnitAssignmentRow
        {
            public Guid OwnerId { get; init; }
            public Guid Id { get; init; }
            public Guid ProductId { get; init; }
            public Guid UnitDefinitionId { get; init; }
            public string? UnitDefinitionCode { get; init; }
            public string? UnitDefinitionName { get; init; }
            public string Code { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public string? Description { get; init; }
            public int Role { get; init; } = 1;
            public bool IsDefault { get; init; }
            public bool IsActive { get; init; }
            public int SortOrder { get; init; }
            public DateTime CreatedAt { get; init; }
            public DateTime? UpdatedAt { get; init; }
        }

        private static IReadOnlyList<Guid> ResolveProductUnitIds(
            IReadOnlyList<Guid>? productUnitIds,
            IReadOnlyList<string>? productUnitTempIds,
            IReadOnlyDictionary<string, Guid>? productUnitTempIdMap)
        {
            var resolvedIds = new List<Guid>();

            if (productUnitIds is not null)
            {
                resolvedIds.AddRange(productUnitIds.Where(id => id != Guid.Empty));
            }

            if (productUnitTempIds is not null && productUnitTempIdMap is not null)
            {
                foreach (var tempId in productUnitTempIds.Where(id => !string.IsNullOrWhiteSpace(id)))
                {
                    if (productUnitTempIdMap.TryGetValue(tempId, out var mappedId))
                    {
                        resolvedIds.Add(mappedId);
                    }
                }
            }

            return resolvedIds
                .Distinct()
                .ToList();
        }

        private static async Task InsertLicenseOfferingUnitAssignmentsAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid offeringId,
            IReadOnlyList<Guid> productUnitIds,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (productUnitIds.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductLicenseOfferingUnits]
 (Id, ProductLicenseOfferingId, ProductUnitId, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductLicenseOfferingId, @ProductUnitId, @Now, 0);";

            var parameters = productUnitIds
                .Distinct()
                .Select(productUnitId => new
                {
                    Id = Guid.NewGuid(),
                    ProductLicenseOfferingId = offeringId,
                    ProductUnitId = productUnitId,
                    Now = now
                });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertPricingRuleUnitAssignmentsAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid pricingRuleId,
            IReadOnlyList<Guid> productUnitIds,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (productUnitIds.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductPricingRuleUnits]
 (Id, ProductPricingRuleId, ProductUnitId, CreatedAt, IsDeleted)
VALUES
 (@Id, @ProductPricingRuleId, @ProductUnitId, @Now, 0);";

            var parameters = productUnitIds
                .Distinct()
                .Select(productUnitId => new
                {
                    Id = Guid.NewGuid(),
                    ProductPricingRuleId = pricingRuleId,
                    ProductUnitId = productUnitId,
                    Now = now
                });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task DeleteLicenseOfferingUnitAssignmentsAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid offeringId,
            CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM [Product].[ProductLicenseOfferingUnits] WHERE ProductLicenseOfferingId = @OfferingId;";
            await connection.ExecuteAsync(new CommandDefinition(sql, new { OfferingId = offeringId }, transaction, cancellationToken: cancellationToken));
        }

        private static async Task DeletePricingRuleUnitAssignmentsAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid pricingRuleId,
            CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM [Product].[ProductPricingRuleUnits] WHERE ProductPricingRuleId = @PricingRuleId;";
            await connection.ExecuteAsync(new CommandDefinition(sql, new { PricingRuleId = pricingRuleId }, transaction, cancellationToken: cancellationToken));
        }

        private static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ProductUnitDto>>> LoadLicenseOfferingUnitsAsync(
            IDbConnection connection,
            IEnumerable<Guid> offeringIds,
            CancellationToken cancellationToken)
        {
            var ids = offeringIds.Distinct().ToArray();
            if (ids.Length == 0)
            {
                return new Dictionary<Guid, IReadOnlyList<ProductUnitDto>>();
            }

            const string sql = @"
SELECT ou.ProductLicenseOfferingId AS OwnerId,
       pu.Id, pu.ProductId, pu.UnitDefinitionId,
       ud.Code AS UnitDefinitionCode, ud.Name AS UnitDefinitionName,
       pu.Code, pu.Name, pu.Description, pu.Role, pu.IsDefault, pu.IsActive,
       pu.SortOrder, pu.CreatedAt, pu.UpdatedAt
FROM [Product].[ProductLicenseOfferingUnits] ou
JOIN [Product].[ProductUnits] pu ON pu.Id = ou.ProductUnitId AND pu.IsDeleted = 0
JOIN [Product].[UnitDefinitions] ud ON ud.Id = pu.UnitDefinitionId AND ud.IsDeleted = 0
WHERE ou.ProductLicenseOfferingId IN @OwnerIds AND ou.IsDeleted = 0
ORDER BY pu.SortOrder, pu.Name;";

            return await LoadProductUnitAssignmentsAsync(connection, sql, ids, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ProductUnitDto>>> LoadPricingRuleUnitsAsync(
            IDbConnection connection,
            IEnumerable<Guid> pricingRuleIds,
            CancellationToken cancellationToken)
        {
            var ids = pricingRuleIds.Distinct().ToArray();
            if (ids.Length == 0)
            {
                return new Dictionary<Guid, IReadOnlyList<ProductUnitDto>>();
            }

            const string sql = @"
SELECT ru.ProductPricingRuleId AS OwnerId,
       pu.Id, pu.ProductId, pu.UnitDefinitionId,
       ud.Code AS UnitDefinitionCode, ud.Name AS UnitDefinitionName,
       pu.Code, pu.Name, pu.Description, pu.Role, pu.IsDefault, pu.IsActive,
       pu.SortOrder, pu.CreatedAt, pu.UpdatedAt
FROM [Product].[ProductPricingRuleUnits] ru
JOIN [Product].[ProductUnits] pu ON pu.Id = ru.ProductUnitId AND pu.IsDeleted = 0
JOIN [Product].[UnitDefinitions] ud ON ud.Id = pu.UnitDefinitionId AND ud.IsDeleted = 0
WHERE ru.ProductPricingRuleId IN @OwnerIds AND ru.IsDeleted = 0
ORDER BY pu.SortOrder, pu.Name;";

            return await LoadProductUnitAssignmentsAsync(connection, sql, ids, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<ProductUnitDto>>> LoadProductUnitAssignmentsAsync(
            IDbConnection connection,
            string sql,
            IReadOnlyList<Guid> ownerIds,
            CancellationToken cancellationToken)
        {
            var rows = await connection.QueryAsync<ProductUnitAssignmentRow>(
                new CommandDefinition(sql, new { OwnerIds = ownerIds }, cancellationToken: cancellationToken));

            return rows
                .GroupBy(row => row.OwnerId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<ProductUnitDto>)group.Select(ToProductUnitDto).ToList());
        }

        private static IReadOnlyList<ProductLicenseOfferingDto> AttachProductUnits(
            IReadOnlyList<ProductLicenseOfferingDto> offerings,
            IReadOnlyDictionary<Guid, IReadOnlyList<ProductUnitDto>> unitsByOfferingId)
            => offerings
                .Select(offering =>
                {
                    var units = unitsByOfferingId.TryGetValue(offering.Id, out var assignedUnits)
                        ? assignedUnits
                        : BuildFallbackProductUnits(offering);

                    return offering with
                    {
                        ProductUnitIds = units.Select(unit => unit.Id).ToList(),
                        ProductUnits = units
                    };
                })
                .ToList();

        private static IReadOnlyList<ProductPricingRuleDto> AttachProductUnits(
            IReadOnlyList<ProductPricingRuleDto> pricingRules,
            IReadOnlyDictionary<Guid, IReadOnlyList<ProductUnitDto>> unitsByPricingRuleId)
            => pricingRules
                .Select(rule =>
                {
                    var units = unitsByPricingRuleId.TryGetValue(rule.Id, out var assignedUnits)
                        ? assignedUnits
                        : BuildFallbackProductUnits(rule);

                    return rule with
                    {
                        ProductUnitIds = units.Select(unit => unit.Id).ToList(),
                        ProductUnits = units
                    };
                })
                .ToList();

        private static ProductUnitDto ToProductUnitDto(ProductUnitAssignmentRow row)
            => new()
            {
                Id = row.Id,
                ProductId = row.ProductId,
                UnitDefinitionId = row.UnitDefinitionId,
                UnitDefinitionCode = row.UnitDefinitionCode,
                UnitDefinitionName = row.UnitDefinitionName,
                Code = row.Code,
                Name = row.Name,
                Description = row.Description,
                Role = row.Role,
                IsDefault = row.IsDefault,
                IsActive = row.IsActive,
                SortOrder = row.SortOrder,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt
            };

        private static IReadOnlyList<ProductUnitDto> BuildFallbackProductUnits(ProductLicenseOfferingDto offering)
            => [];

        private static IReadOnlyList<ProductUnitDto> BuildFallbackProductUnits(ProductPricingRuleDto rule)
            => [];
    }
}
