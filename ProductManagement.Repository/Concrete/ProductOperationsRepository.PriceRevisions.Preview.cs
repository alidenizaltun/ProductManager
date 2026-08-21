using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Exceptions;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProductManagement.Repository.Concrete
{
    /// <summary>
    /// Önizleme üretimi. Kapsam satırları iki role ayrılır:
    /// <list type="bullet">
    /// <item><b>Ürün filtresi</b> (ürün, kategori, ürün tipi, bölge) hangi ürünlerin ele alınacağını,</item>
    /// <item><b>Hedef filtresi</b> (şablon, birim, satış planı, fiyat listesi) hangi fiyat satırlarının
    /// ele alınacağını belirler.</item>
    /// </list>
    /// Hiç ürün filtresi yoksa bütün ürünler, hiç hedef filtresi yoksa altı fiyat alanının
    /// tamamı kapsama girer. Ayrım şart: "SMS şablonuna zam" dendiğinde ürünün paket taban
    /// fiyatına dokunulmamalıdır.
    /// </summary>
    public sealed partial class ProductOperationsRepository
    {
        private const int PriceRevisionStatusDraft = 1;
        private const int PriceRevisionStatusPreviewed = 2;

        private const int AdjustmentTypePercent = 1;
        private const int AdjustmentTypeAmount = 2;
        private const int AdjustmentTypeSetValue = 3;
        private const int AdjustmentTypeMultiplier = 4;

        private const int RoundingModeNone = 1;
        private const int RoundingModeRound = 2;
        private const int RoundingModeCeiling = 3;
        private const int RoundingModeFloor = 4;

        private const int ScopeTypeProduct = 1;
        private const int ScopeTypeCategory = 2;
        private const int ScopeTypePricingTemplate = 3;
        private const int ScopeTypeUnitDefinition = 4;
        private const int ScopeTypeLicenseOffering = 5;
        private const int ScopeTypePriceList = 6;
        private const int ScopeTypeProductKind = 7;
        private const int ScopeTypeRegion = 8;

        private const int TargetTypeLicenseOfferingBasePrice = 1;
        private const int TargetTypeModuleOfferingPrice = 2;
        private const int TargetTypePricingRuleValue = 3;
        private const int TargetTypePricingRuleTier = 4;
        private const int TargetTypeProductPrice = 5;
        private const int TargetTypePriceListItem = 6;

        public async Task<PriceRevisionSummaryDto> PreviewPriceRevisionAsync(
            Guid priceRevisionId,
            CancellationToken cancellationToken = default)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var revision = await LoadPriceRevisionHeaderAsync(connection, transaction, priceRevisionId, cancellationToken);
                var scopes = await LoadPriceRevisionScopesAsync(connection, transaction, priceRevisionId, cancellationToken);
                var filters = BuildScopeFilters(scopes);

                // Kullanıcının elle hariç tuttuğu satırlar önizleme yenilenince kaybolmamalı.
                var previouslyExcluded = await LoadExcludedLineKeysAsync(connection, transaction, priceRevisionId, cancellationToken);

                var lines = new List<PriceRevisionLineDraft>();
                var skippedRules = new List<PriceRevisionSkippedRuleDto>();

                if (filters.IncludesTargetType(TargetTypeLicenseOfferingBasePrice))
                {
                    lines.AddRange(await ScanLicenseOfferingBasePricesAsync(connection, transaction, revision, filters, cancellationToken));
                }

                if (filters.IncludesTargetType(TargetTypeModuleOfferingPrice))
                {
                    lines.AddRange(await ScanModuleOfferingPricesAsync(connection, transaction, revision, filters, cancellationToken));
                }

                if (filters.IncludesTargetType(TargetTypePricingRuleValue))
                {
                    lines.AddRange(await ScanPricingRulesAsync(connection, transaction, revision, filters, skippedRules, cancellationToken));
                }

                if (filters.IncludesTargetType(TargetTypeProductPrice))
                {
                    lines.AddRange(await ScanProductPricesAsync(connection, transaction, revision, filters, cancellationToken));
                }

                if (filters.IncludesTargetType(TargetTypePriceListItem))
                {
                    lines.AddRange(await ScanPriceListItemsAsync(connection, transaction, revision, filters, cancellationToken));
                }

                await ReplacePriceRevisionLinesAsync(
                    connection, transaction, priceRevisionId, revision, lines, previouslyExcluded, cancellationToken);

                await connection.ExecuteAsync(new CommandDefinition(
                    @"UPDATE [Product].[PriceRevisions] SET Status = @Status, UpdatedAt = @Now WHERE Id = @PriceRevisionId;",
                    new { PriceRevisionId = priceRevisionId, Status = PriceRevisionStatusPreviewed, Now = DateTime.UtcNow },
                    transaction, cancellationToken: cancellationToken));

                var summary = await LoadPriceRevisionSummaryAsync(connection, transaction, priceRevisionId, cancellationToken);
                transaction.Commit();
                return summary with { SkippedRules = skippedRules };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static async Task<PriceRevisionHeader> LoadPriceRevisionHeaderAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid priceRevisionId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT Id, AdjustmentType, Value, RoundingMode, RoundingStep, CurrencyCode, Status
FROM [Product].[PriceRevisions]
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            return await connection.QuerySingleOrDefaultAsync<PriceRevisionHeader>(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken))
                ?? throw new NotFoundException("Fiyat revizyonu bulunamadı.");
        }

        private static async Task<HashSet<string>> LoadExcludedLineKeysAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid priceRevisionId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT TargetType, TargetId, TargetPath
FROM [Product].[PriceRevisionLines]
WHERE PriceRevisionId = @PriceRevisionId AND IsExcluded = 1;";

            // Dapper ValueTuple'a eşleme yapamaz; adlandırılmış bir tip şart.
            var rows = await connection.QueryAsync<ExcludedLineKeyRow>(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));

            return rows
                .Select(row => BuildLineKey(row.TargetType, row.TargetId, row.TargetPath))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static string BuildLineKey(int targetType, Guid targetId, string targetPath)
            => $"{targetType}|{targetId}|{targetPath}";

        // ─── Kapsam çözümlemesi ──────────────────────────────────────────────────────

        private static ScopeFilters BuildScopeFilters(IReadOnlyList<PriceRevisionScopeDto> scopes)
        {
            var filters = new ScopeFilters();

            foreach (var scope in scopes)
            {
                var bucket = scope.IsExclude ? filters.Excluded : filters.Included;

                switch (scope.ScopeType)
                {
                    case ScopeTypeProduct when scope.TargetId.HasValue:
                        bucket.ProductIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypeCategory when scope.TargetId.HasValue:
                        bucket.CategoryIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypeRegion when scope.TargetId.HasValue:
                        bucket.RegionIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypeProductKind when int.TryParse(scope.TargetValue, out var kind):
                        bucket.ProductKinds.Add(kind);
                        break;
                    case ScopeTypePricingTemplate when scope.TargetId.HasValue:
                        bucket.PricingTemplateIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypeUnitDefinition when scope.TargetId.HasValue:
                        bucket.UnitDefinitionIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypeLicenseOffering when scope.TargetId.HasValue:
                        bucket.LicenseOfferingIds.Add(scope.TargetId.Value);
                        break;
                    case ScopeTypePriceList when scope.TargetId.HasValue:
                        bucket.PriceListIds.Add(scope.TargetId.Value);
                        break;
                }
            }

            return filters;
        }

        /// <summary>
        /// Ürün filtrelerini WHERE parçasına çevirir. Aynı parametre adları bütün
        /// tarama sorgularında kullanılır; <paramref name="productColumn"/> her sorguda
        /// ürün kimliğini taşıyan kolonun adıdır.
        /// </summary>
        private static void AppendProductScope(StringBuilder sql, ScopeFilters filters, string productColumn)
        {
            var included = filters.Included;
            var excluded = filters.Excluded;

            if (included.HasProductFilter)
            {
                sql.Append($@"
  AND (
        (@HasIncludedProducts = 1 AND {productColumn} IN @IncludedProductIds)
     OR (@HasIncludedCategories = 1 AND EXISTS (
            SELECT 1 FROM [Product].[ProductCategoryMaps] cm
            WHERE cm.ProductId = {productColumn} AND cm.IsDeleted = 0 AND cm.ProductCategoryId IN @IncludedCategoryIds))
     OR (@HasIncludedRegions = 1 AND EXISTS (
            SELECT 1 FROM [Product].[ProductRegions] pgr
            WHERE pgr.ProductId = {productColumn} AND pgr.IsDeleted = 0 AND pgr.RegionId IN @IncludedRegionIds))
     OR (@HasIncludedKinds = 1 AND EXISTS (
            SELECT 1 FROM [Product].[Products] kp
            WHERE kp.Id = {productColumn} AND kp.Kind IN @IncludedProductKinds))
      )");
            }

            if (excluded.ProductIds.Count > 0)
            {
                sql.Append($@"
  AND {productColumn} NOT IN @ExcludedProductIds");
            }

            if (excluded.CategoryIds.Count > 0)
            {
                sql.Append($@"
  AND NOT EXISTS (
        SELECT 1 FROM [Product].[ProductCategoryMaps] xcm
        WHERE xcm.ProductId = {productColumn} AND xcm.IsDeleted = 0 AND xcm.ProductCategoryId IN @ExcludedCategoryIds)");
            }

            if (excluded.ProductKinds.Count > 0)
            {
                sql.Append($@"
  AND NOT EXISTS (
        SELECT 1 FROM [Product].[Products] xkp
        WHERE xkp.Id = {productColumn} AND xkp.Kind IN @ExcludedProductKinds)");
            }
        }

        private static DynamicParameters BuildScopeParameters(ScopeFilters filters, PriceRevisionHeader revision)
        {
            var parameters = new DynamicParameters();
            parameters.Add("CurrencyCode", revision.CurrencyCode);
            parameters.Add("Now", DateTime.UtcNow);

            parameters.Add("HasIncludedProducts", filters.Included.ProductIds.Count > 0);
            parameters.Add("HasIncludedCategories", filters.Included.CategoryIds.Count > 0);
            parameters.Add("HasIncludedRegions", filters.Included.RegionIds.Count > 0);
            parameters.Add("HasIncludedKinds", filters.Included.ProductKinds.Count > 0);

            // Dapper boş listeyi genişletemez; her zaman en az bir eşleşmeyen değer bulunmalı.
            parameters.Add("IncludedProductIds", NonEmpty(filters.Included.ProductIds));
            parameters.Add("IncludedCategoryIds", NonEmpty(filters.Included.CategoryIds));
            parameters.Add("IncludedRegionIds", NonEmpty(filters.Included.RegionIds));
            parameters.Add("IncludedProductKinds", NonEmpty(filters.Included.ProductKinds));

            parameters.Add("ExcludedProductIds", NonEmpty(filters.Excluded.ProductIds));
            parameters.Add("ExcludedCategoryIds", NonEmpty(filters.Excluded.CategoryIds));
            parameters.Add("ExcludedProductKinds", NonEmpty(filters.Excluded.ProductKinds));

            parameters.Add("IncludedTemplateIds", NonEmpty(filters.Included.PricingTemplateIds));
            parameters.Add("ExcludedTemplateIds", NonEmpty(filters.Excluded.PricingTemplateIds));
            parameters.Add("IncludedUnitDefinitionIds", NonEmpty(filters.Included.UnitDefinitionIds));
            parameters.Add("ExcludedUnitDefinitionIds", NonEmpty(filters.Excluded.UnitDefinitionIds));
            parameters.Add("IncludedOfferingIds", NonEmpty(filters.Included.LicenseOfferingIds));
            parameters.Add("ExcludedOfferingIds", NonEmpty(filters.Excluded.LicenseOfferingIds));
            parameters.Add("IncludedPriceListIds", NonEmpty(filters.Included.PriceListIds));
            parameters.Add("ExcludedPriceListIds", NonEmpty(filters.Excluded.PriceListIds));

            return parameters;
        }

        private static IReadOnlyList<Guid> NonEmpty(HashSet<Guid> values)
            => values.Count > 0 ? values.ToList() : [Guid.Empty];

        private static IReadOnlyList<int> NonEmpty(HashSet<int> values)
            => values.Count > 0 ? values.ToList() : [int.MinValue];

        // ─── Hedef taramaları ────────────────────────────────────────────────────────

        private static async Task<IReadOnlyList<PriceRevisionLineDraft>> ScanLicenseOfferingBasePricesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            PriceRevisionHeader revision,
            ScopeFilters filters,
            CancellationToken cancellationToken)
        {
            var sql = new StringBuilder(@"
SELECT o.Id AS TargetId, o.ProductId, p.Name AS ProductName,
       o.Name AS TargetLabel, o.BasePrice AS OldValue, o.CurrencyCode
FROM [Product].[ProductLicenseOfferings] o
JOIN [Product].[Products] p ON p.Id = o.ProductId AND p.IsDeleted = 0
WHERE o.IsDeleted = 0 AND o.IsActive = 1
  AND (@CurrencyCode IS NULL OR o.CurrencyCode = @CurrencyCode)");

            AppendProductScope(sql, filters, "o.ProductId");

            if (filters.Included.LicenseOfferingIds.Count > 0)
            {
                sql.Append(@"
  AND o.Id IN @IncludedOfferingIds");
            }

            if (filters.Excluded.LicenseOfferingIds.Count > 0)
            {
                sql.Append(@"
  AND o.Id NOT IN @ExcludedOfferingIds");
            }

            sql.Append(';');

            var rows = await connection.QueryAsync<PriceTargetRow>(
                new CommandDefinition(sql.ToString(), BuildScopeParameters(filters, revision), transaction, cancellationToken: cancellationToken));

            return rows.Select(row => new PriceRevisionLineDraft
            {
                TargetType = TargetTypeLicenseOfferingBasePrice,
                TargetId = row.TargetId,
                TargetPath = string.Empty,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                TargetLabel = $"Paket taban fiyatı · {row.TargetLabel}",
                CurrencyCode = row.CurrencyCode,
                OldValue = row.OldValue
            }).ToList();
        }

        private static async Task<IReadOnlyList<PriceRevisionLineDraft>> ScanModuleOfferingPricesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            PriceRevisionHeader revision,
            ScopeFilters filters,
            CancellationToken cancellationToken)
        {
            var sql = new StringBuilder(@"
SELECT mp.Id AS TargetId, m.ProductId, p.Name AS ProductName,
       CONCAT(m.Name, ' / ', ISNULL(o.Name, '-')) AS TargetLabel,
       mp.Price AS OldValue, mp.CurrencyCode
FROM [Product].[ProductModuleOfferingPrices] mp
JOIN [Product].[ProductModules] m ON m.Id = mp.ProductModuleId AND m.IsDeleted = 0
JOIN [Product].[Products] p ON p.Id = m.ProductId AND p.IsDeleted = 0
LEFT JOIN [Product].[ProductLicenseOfferings] o ON o.Id = mp.ProductLicenseOfferingId AND o.IsDeleted = 0
WHERE mp.IsDeleted = 0 AND mp.IsActive = 1
  AND (@CurrencyCode IS NULL OR mp.CurrencyCode = @CurrencyCode)");

            AppendProductScope(sql, filters, "m.ProductId");

            if (filters.Included.LicenseOfferingIds.Count > 0)
            {
                sql.Append(@"
  AND mp.ProductLicenseOfferingId IN @IncludedOfferingIds");
            }

            if (filters.Excluded.LicenseOfferingIds.Count > 0)
            {
                sql.Append(@"
  AND mp.ProductLicenseOfferingId NOT IN @ExcludedOfferingIds");
            }

            sql.Append(';');

            var rows = await connection.QueryAsync<PriceTargetRow>(
                new CommandDefinition(sql.ToString(), BuildScopeParameters(filters, revision), transaction, cancellationToken: cancellationToken));

            return rows.Select(row => new PriceRevisionLineDraft
            {
                TargetType = TargetTypeModuleOfferingPrice,
                TargetId = row.TargetId,
                TargetPath = string.Empty,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                TargetLabel = $"Modül fiyatı · {row.TargetLabel}",
                CurrencyCode = row.CurrencyCode,
                OldValue = row.OldValue
            }).ToList();
        }

        /// <summary>
        /// Ürün fiyatlarında yalnızca bugün geçerli olan satırlar taranır; geçmiş
        /// dönem fiyatlarını zamlamak istenmez.
        /// </summary>
        private static async Task<IReadOnlyList<PriceRevisionLineDraft>> ScanProductPricesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            PriceRevisionHeader revision,
            ScopeFilters filters,
            CancellationToken cancellationToken)
        {
            var sql = new StringBuilder(@"
SELECT pp.Id AS TargetId, pp.ProductId, p.Name AS ProductName,
       CONCAT('Fiyat tipi ', pp.PriceType, ISNULL(' · ' + v.Sku, '')) AS TargetLabel,
       pp.Amount AS OldValue, pp.CurrencyCode
FROM [Product].[ProductPrices] pp
JOIN [Product].[Products] p ON p.Id = pp.ProductId AND p.IsDeleted = 0
LEFT JOIN [Product].[ProductVariants] v ON v.Id = pp.ProductVariantId AND v.IsDeleted = 0
WHERE pp.IsDeleted = 0
  AND (pp.ValidFrom IS NULL OR pp.ValidFrom <= @Now)
  AND (pp.ValidTo IS NULL OR pp.ValidTo >= @Now)
  AND (@CurrencyCode IS NULL OR pp.CurrencyCode = @CurrencyCode)");

            AppendProductScope(sql, filters, "pp.ProductId");

            if (filters.Included.RegionIds.Count > 0)
            {
                sql.Append(@"
  AND (pp.RegionId IS NULL OR pp.RegionId IN @IncludedRegionIds)");
            }

            sql.Append(';');

            var rows = await connection.QueryAsync<PriceTargetRow>(
                new CommandDefinition(sql.ToString(), BuildScopeParameters(filters, revision), transaction, cancellationToken: cancellationToken));

            return rows.Select(row => new PriceRevisionLineDraft
            {
                TargetType = TargetTypeProductPrice,
                TargetId = row.TargetId,
                TargetPath = string.Empty,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                TargetLabel = $"Ürün fiyatı · {row.TargetLabel}",
                CurrencyCode = row.CurrencyCode,
                OldValue = row.OldValue
            }).ToList();
        }

        private static async Task<IReadOnlyList<PriceRevisionLineDraft>> ScanPriceListItemsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            PriceRevisionHeader revision,
            ScopeFilters filters,
            CancellationToken cancellationToken)
        {
            var sql = new StringBuilder(@"
SELECT i.Id AS TargetId, i.ProductId, p.Name AS ProductName,
       pl.Name AS TargetLabel, i.Amount AS OldValue, pl.CurrencyCode
FROM [Product].[ProductPriceListItems] i
JOIN [Product].[ProductPriceLists] pl ON pl.Id = i.ProductPriceListId AND pl.IsDeleted = 0
JOIN [Product].[Products] p ON p.Id = i.ProductId AND p.IsDeleted = 0
WHERE i.IsDeleted = 0 AND pl.IsActive = 1
  AND (@CurrencyCode IS NULL OR pl.CurrencyCode = @CurrencyCode)");

            AppendProductScope(sql, filters, "i.ProductId");

            if (filters.Included.PriceListIds.Count > 0)
            {
                sql.Append(@"
  AND i.ProductPriceListId IN @IncludedPriceListIds");
            }

            if (filters.Excluded.PriceListIds.Count > 0)
            {
                sql.Append(@"
  AND i.ProductPriceListId NOT IN @ExcludedPriceListIds");
            }

            sql.Append(';');

            var rows = await connection.QueryAsync<PriceTargetRow>(
                new CommandDefinition(sql.ToString(), BuildScopeParameters(filters, revision), transaction, cancellationToken: cancellationToken));

            return rows.Select(row => new PriceRevisionLineDraft
            {
                TargetType = TargetTypePriceListItem,
                TargetId = row.TargetId,
                TargetPath = string.Empty,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                TargetLabel = $"Liste satırı · {row.TargetLabel}",
                CurrencyCode = row.CurrencyCode,
                OldValue = row.OldValue
            }).ToList();
        }

        /// <summary>
        /// Fiyatlandırma kuralları. Kural gövdesi JSON olduğu için tek bir kuraldan
        /// birden çok satır çıkabilir (her kademe ayrı bir satır). Oran ve indirim
        /// tipli kurallar bir fiyat taşımadığı için zam kapsamı dışında bırakılır.
        /// </summary>
        private static async Task<IReadOnlyList<PriceRevisionLineDraft>> ScanPricingRulesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            PriceRevisionHeader revision,
            ScopeFilters filters,
            List<PriceRevisionSkippedRuleDto> skippedRules,
            CancellationToken cancellationToken)
        {
            var sql = new StringBuilder(@"
SELECT r.Id, r.ProductId, p.Name AS ProductName, r.Name AS RuleName, r.Code AS RuleCode,
       r.PriceAdjustmentJson, p.DefaultCurrencyCode AS CurrencyCode
FROM [Product].[ProductPricingRules] r
JOIN [Product].[Products] p ON p.Id = r.ProductId AND p.IsDeleted = 0
WHERE r.IsDeleted = 0 AND r.IsActive = 1
  AND (@CurrencyCode IS NULL OR p.DefaultCurrencyCode = @CurrencyCode)");

            AppendProductScope(sql, filters, "r.ProductId");

            if (filters.Included.PricingTemplateIds.Count > 0)
            {
                sql.Append(@"
  AND r.SourceTemplateId IN @IncludedTemplateIds");
            }

            if (filters.Excluded.PricingTemplateIds.Count > 0)
            {
                sql.Append(@"
  AND (r.SourceTemplateId IS NULL OR r.SourceTemplateId NOT IN @ExcludedTemplateIds)");
            }

            if (filters.Included.UnitDefinitionIds.Count > 0)
            {
                // Bir kural birime iki yoldan bağlı olabilir: doğrudan atanmış ürün birimi
                // ya da geldiği şablonun birim tanımı. Şablondan uygulanan kurallara birim
                // atanmadığı için ikinci yol olmazsa bu kapsam onları hiç bulamaz.
                sql.Append(@"
  AND (
        EXISTS (
            SELECT 1
            FROM [Product].[ProductPricingRuleUnits] ru
            JOIN [Product].[ProductUnits] pu ON pu.Id = ru.ProductUnitId AND pu.IsDeleted = 0
            WHERE ru.ProductPricingRuleId = r.Id AND ru.IsDeleted = 0
              AND pu.UnitDefinitionId IN @IncludedUnitDefinitionIds)
     OR EXISTS (
            SELECT 1
            FROM [Product].[PricingTemplates] t
            WHERE t.Id = r.SourceTemplateId AND t.IsDeleted = 0
              AND t.UnitDefinitionId IN @IncludedUnitDefinitionIds)
      )");
            }

            if (filters.Excluded.UnitDefinitionIds.Count > 0)
            {
                sql.Append(@"
  AND NOT EXISTS (
        SELECT 1
        FROM [Product].[ProductPricingRuleUnits] xru
        JOIN [Product].[ProductUnits] xpu ON xpu.Id = xru.ProductUnitId AND xpu.IsDeleted = 0
        WHERE xru.ProductPricingRuleId = r.Id AND xru.IsDeleted = 0
          AND xpu.UnitDefinitionId IN @ExcludedUnitDefinitionIds)
  AND NOT EXISTS (
        SELECT 1
        FROM [Product].[PricingTemplates] xt
        WHERE xt.Id = r.SourceTemplateId AND xt.IsDeleted = 0
          AND xt.UnitDefinitionId IN @ExcludedUnitDefinitionIds)");
            }

            if (filters.Included.LicenseOfferingIds.Count > 0)
            {
                sql.Append(@"
  AND (r.ProductLicenseOfferingId IS NULL OR r.ProductLicenseOfferingId IN @IncludedOfferingIds)");
            }

            sql.Append(';');

            var rules = await connection.QueryAsync<PricingRuleTargetRow>(
                new CommandDefinition(sql.ToString(), BuildScopeParameters(filters, revision), transaction, cancellationToken: cancellationToken));

            var lines = new List<PriceRevisionLineDraft>();

            foreach (var rule in rules)
            {
                var extraction = ExtractAdjustableValues(rule);

                if (extraction.SkipReason is not null)
                {
                    skippedRules.Add(new PriceRevisionSkippedRuleDto
                    {
                        PricingRuleId = rule.Id,
                        ProductId = rule.ProductId,
                        ProductName = rule.ProductName,
                        PricingRuleName = rule.RuleName,
                        Reason = extraction.SkipReason
                    });
                    continue;
                }

                lines.AddRange(extraction.Values.Select(value => new PriceRevisionLineDraft
                {
                    TargetType = value.IsTier ? TargetTypePricingRuleTier : TargetTypePricingRuleValue,
                    TargetId = rule.Id,
                    TargetPath = value.Path,
                    ProductId = rule.ProductId,
                    ProductName = rule.ProductName,
                    TargetLabel = value.Label(rule.RuleName, rule.RuleCode),
                    CurrencyCode = rule.CurrencyCode,
                    OldValue = value.Amount
                }));
            }

            return lines;
        }

        /// <summary>
        /// Kural gövdesinden zamlanabilir tutarları çıkarır. Yüzde, çarpan ve indirim
        /// (<c>operation: subtract</c>) kuralları bir tutar değil bir oran ifade ettiği
        /// için atlanır; bunlara zam uygulamak fiyatı bozar.
        /// </summary>
        private static AdjustableValueExtraction ExtractAdjustableValues(PricingRuleTargetRow rule)
        {
            JsonNode? node;
            try
            {
                node = JsonNode.Parse(rule.PriceAdjustmentJson);
            }
            catch (JsonException)
            {
                return AdjustableValueExtraction.Skipped("Kural içeriği okunamadı.");
            }

            if (node is not JsonObject adjustment)
            {
                return AdjustableValueExtraction.Skipped("Kural içeriği bir nesne değil.");
            }

            if (string.Equals(ReadString(adjustment, "operation"), "subtract", StringComparison.OrdinalIgnoreCase))
            {
                return AdjustableValueExtraction.Skipped("İndirim kuralı; zam uygulanmaz.");
            }

            var values = new List<AdjustableValue>();

            if (adjustment["tiers"] is JsonArray tiers && tiers.Count > 0)
            {
                for (var index = 0; index < tiers.Count; index++)
                {
                    if (tiers[index] is not JsonObject tier)
                    {
                        continue;
                    }

                    var tierType = ReadString(tier, "type") ?? ReadString(adjustment, "type");
                    if (IsRatioType(tierType))
                    {
                        continue;
                    }

                    if (TryReadDecimal(tier, "value", out var tierValue))
                    {
                        values.Add(new AdjustableValue($"$.tiers[{index}].value", tierValue, IsTier: true, TierIndex: index));
                    }
                }

                return values.Count > 0
                    ? AdjustableValueExtraction.Found(values)
                    : AdjustableValueExtraction.Skipped("Kademelerin tamamı oran tipli; zam uygulanmaz.");
            }

            if (IsRatioType(ReadString(adjustment, "type")))
            {
                return AdjustableValueExtraction.Skipped("Oran tipli kural; zam uygulanmaz.");
            }

            if (TryReadDecimal(adjustment, "value", out var value))
            {
                values.Add(new AdjustableValue("$.value", value, IsTier: false, TierIndex: null));
            }
            else if (TryReadDecimal(adjustment, "amount", out var amount))
            {
                values.Add(new AdjustableValue("$.amount", amount, IsTier: false, TierIndex: null));
            }

            return values.Count > 0
                ? AdjustableValueExtraction.Found(values)
                : AdjustableValueExtraction.Skipped("Kuralda zamlanabilir bir tutar bulunamadı.");
        }

        private static bool IsRatioType(string? type)
            => type is not null
            && (type.Equals("percent", StringComparison.OrdinalIgnoreCase)
                || type.Equals("percentage", StringComparison.OrdinalIgnoreCase)
                || type.Equals("multiplier", StringComparison.OrdinalIgnoreCase));

        private static string? ReadString(JsonObject source, string propertyName)
        {
            var key = source.Select(property => property.Key)
                .FirstOrDefault(property => string.Equals(property, propertyName, StringComparison.OrdinalIgnoreCase));

            if (key is null || source[key] is not JsonValue jsonValue)
            {
                return null;
            }

            return jsonValue.GetValueKind() == JsonValueKind.String ? jsonValue.GetValue<string>() : null;
        }

        private static bool TryReadDecimal(JsonObject source, string propertyName, out decimal value)
        {
            value = 0;
            var key = source.Select(property => property.Key)
                .FirstOrDefault(property => string.Equals(property, propertyName, StringComparison.OrdinalIgnoreCase));

            return key is not null
                && source[key] is JsonValue jsonValue
                && jsonValue.TryGetValue(out value);
        }

        // ─── Yeni fiyat hesabı ───────────────────────────────────────────────────────

        internal static decimal CalculateNewValue(decimal oldValue, PriceRevisionHeader revision)
        {
            var raw = revision.AdjustmentType switch
            {
                AdjustmentTypePercent => oldValue * (1 + revision.Value / 100m),
                AdjustmentTypeAmount => oldValue + revision.Value,
                AdjustmentTypeSetValue => revision.Value,
                AdjustmentTypeMultiplier => oldValue * revision.Value,
                _ => oldValue
            };

            return ApplyRounding(raw, revision.RoundingMode, revision.RoundingStep);
        }

        private static decimal ApplyRounding(decimal value, int roundingMode, decimal? step)
        {
            if (roundingMode == RoundingModeNone)
            {
                return decimal.Round(value, 4, MidpointRounding.AwayFromZero);
            }

            var roundingStep = step is > 0 ? step.Value : 0.01m;
            var scaled = value / roundingStep;

            var rounded = roundingMode switch
            {
                RoundingModeRound => Math.Round(scaled, MidpointRounding.AwayFromZero),
                RoundingModeCeiling => Math.Ceiling(scaled),
                RoundingModeFloor => Math.Floor(scaled),
                _ => scaled
            };

            return decimal.Round(rounded * roundingStep, 4, MidpointRounding.AwayFromZero);
        }

        private static async Task ReplacePriceRevisionLinesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid priceRevisionId,
            PriceRevisionHeader revision,
            IReadOnlyList<PriceRevisionLineDraft> drafts,
            HashSet<string> previouslyExcluded,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                @"DELETE FROM [Product].[PriceRevisionLines] WHERE PriceRevisionId = @PriceRevisionId;",
                new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));

            if (drafts.Count == 0)
            {
                return;
            }

            const string insertSql = @"
INSERT INTO [Product].[PriceRevisionLines]
    (Id, PriceRevisionId, TargetType, TargetId, TargetPath, ProductId, ProductName,
     TargetLabel, CurrencyCode, OldValue, NewValue, IsExcluded, IsApplied, CreatedAt, IsDeleted)
VALUES
    (@Id, @PriceRevisionId, @TargetType, @TargetId, @TargetPath, @ProductId, @ProductName,
     @TargetLabel, @CurrencyCode, @OldValue, @NewValue, @IsExcluded, 0, @Now, 0);";

            var now = DateTime.UtcNow;
            var parameters = drafts
                .DistinctBy(draft => BuildLineKey(draft.TargetType, draft.TargetId, draft.TargetPath))
                .Select(draft => new
                {
                    Id = Guid.NewGuid(),
                    PriceRevisionId = priceRevisionId,
                    draft.TargetType,
                    draft.TargetId,
                    draft.TargetPath,
                    draft.ProductId,
                    draft.ProductName,
                    draft.TargetLabel,
                    draft.CurrencyCode,
                    draft.OldValue,
                    NewValue = CalculateNewValue(draft.OldValue, revision),
                    IsExcluded = previouslyExcluded.Contains(
                        BuildLineKey(draft.TargetType, draft.TargetId, draft.TargetPath)),
                    Now = now
                })
                .ToList();

            await connection.ExecuteAsync(
                new CommandDefinition(insertSql, parameters, transaction, cancellationToken: cancellationToken));
        }

        // ─── Yardımcı tipler ─────────────────────────────────────────────────────────

        internal sealed record PriceRevisionHeader
        {
            public Guid Id { get; init; }
            public int AdjustmentType { get; init; }
            public decimal Value { get; init; }
            public int RoundingMode { get; init; }
            public decimal? RoundingStep { get; init; }
            public string? CurrencyCode { get; init; }
            public int Status { get; init; }
        }

        private sealed record ExcludedLineKeyRow
        {
            public int TargetType { get; init; }
            public Guid TargetId { get; init; }
            public string TargetPath { get; init; } = string.Empty;
        }

        private sealed record PriceTargetRow
        {
            public Guid TargetId { get; init; }
            public Guid ProductId { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string TargetLabel { get; init; } = string.Empty;
            public decimal OldValue { get; init; }
            public string CurrencyCode { get; init; } = "TRY";
        }

        private sealed record PricingRuleTargetRow
        {
            public Guid Id { get; init; }
            public Guid ProductId { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string RuleName { get; init; } = string.Empty;
            public string RuleCode { get; init; } = string.Empty;
            public string PriceAdjustmentJson { get; init; } = string.Empty;
            public string CurrencyCode { get; init; } = "TRY";
        }

        private sealed record AdjustableValue(string Path, decimal Amount, bool IsTier, int? TierIndex)
        {
            /// <summary>
            /// Kod da yazılır: aynı ürüne aynı şablon iki kez uygulanmışsa kural adları
            /// aynı olur, satırlar yalnızca kodla ayırt edilebilir.
            /// </summary>
            public string Label(string ruleName, string ruleCode)
                => IsTier
                    ? $"{ruleName} ({ruleCode}) · kademe {TierIndex + 1}"
                    : $"{ruleName} ({ruleCode})";
        }

        private sealed record AdjustableValueExtraction
        {
            public IReadOnlyList<AdjustableValue> Values { get; init; } = [];
            public string? SkipReason { get; init; }

            public static AdjustableValueExtraction Found(IReadOnlyList<AdjustableValue> values)
                => new() { Values = values };

            public static AdjustableValueExtraction Skipped(string reason)
                => new() { SkipReason = reason };
        }

        private sealed class ScopeBucket
        {
            public HashSet<Guid> ProductIds { get; } = [];
            public HashSet<Guid> CategoryIds { get; } = [];
            public HashSet<Guid> RegionIds { get; } = [];
            public HashSet<int> ProductKinds { get; } = [];
            public HashSet<Guid> PricingTemplateIds { get; } = [];
            public HashSet<Guid> UnitDefinitionIds { get; } = [];
            public HashSet<Guid> LicenseOfferingIds { get; } = [];
            public HashSet<Guid> PriceListIds { get; } = [];

            public bool HasProductFilter
                => ProductIds.Count > 0 || CategoryIds.Count > 0 || RegionIds.Count > 0 || ProductKinds.Count > 0;

            public bool HasTargetFilter
                => PricingTemplateIds.Count > 0 || UnitDefinitionIds.Count > 0
                || LicenseOfferingIds.Count > 0 || PriceListIds.Count > 0;
        }

        private sealed class ScopeFilters
        {
            public ScopeBucket Included { get; } = new();
            public ScopeBucket Excluded { get; } = new();

            /// <summary>
            /// Hedef filtresi verilmişse yalnızca o filtrenin işaret ettiği fiyat alanları
            /// taranır. Hiç hedef filtresi yoksa altı alanın tamamı kapsama girer.
            /// </summary>
            public bool IncludesTargetType(int targetType)
            {
                if (!Included.HasTargetFilter)
                {
                    return true;
                }

                var ruleScoped = Included.PricingTemplateIds.Count > 0 || Included.UnitDefinitionIds.Count > 0;
                var offeringScoped = Included.LicenseOfferingIds.Count > 0;
                var priceListScoped = Included.PriceListIds.Count > 0;

                return targetType switch
                {
                    TargetTypePricingRuleValue or TargetTypePricingRuleTier => ruleScoped || offeringScoped,
                    TargetTypeLicenseOfferingBasePrice or TargetTypeModuleOfferingPrice => offeringScoped,
                    TargetTypePriceListItem => priceListScoped,
                    TargetTypeProductPrice => false,
                    _ => false
                };
            }
        }

        private sealed record PriceRevisionLineDraft
        {
            public int TargetType { get; init; }
            public Guid TargetId { get; init; }
            public string TargetPath { get; init; } = string.Empty;
            public Guid ProductId { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string TargetLabel { get; init; } = string.Empty;
            public string CurrencyCode { get; init; } = "TRY";
            public decimal OldValue { get; init; }
        }
    }
}
