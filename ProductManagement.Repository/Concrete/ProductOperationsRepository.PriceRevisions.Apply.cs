using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using System.Data;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ProductManagement.Repository.Concrete
{
    /// <summary>
    /// Revizyonun uygulanması ve geri alınması. İkisi de aynı gövdeyi kullanır:
    /// tek fark, satırın hangi ucundan hangi ucuna gidildiğidir. Her satır yazılmadan
    /// önce hedefteki güncel değerin beklenen değerle aynı olduğu doğrulanır; arada elle
    /// değişmiş bir fiyat sessizce ezilmez, atlanır ve sonuçta bildirilir.
    /// </summary>
    public sealed partial class ProductOperationsRepository
    {
        private const int PriceRevisionStatusApplied = 5;
        private const int PriceRevisionStatusRolledBack = 6;

        private static readonly Regex TierPathPattern = new(
            @"^\$\.tiers\[(?<index>\d+)\]\.(?<field>\w+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public Task<PriceRevisionExecutionResultDto> ApplyPriceRevisionAsync(
            Guid priceRevisionId,
            Guid? userId,
            CancellationToken cancellationToken = default)
            => ExecutePriceRevisionAsync(priceRevisionId, rollback: false, userId, cancellationToken);

        public Task<PriceRevisionExecutionResultDto> RollbackPriceRevisionAsync(
            Guid priceRevisionId,
            Guid? userId,
            CancellationToken cancellationToken = default)
            => ExecutePriceRevisionAsync(priceRevisionId, rollback: true, userId, cancellationToken);

        private async Task<PriceRevisionExecutionResultDto> ExecutePriceRevisionAsync(
            Guid priceRevisionId,
            bool rollback,
            Guid? userId,
            CancellationToken cancellationToken)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var lines = await LoadExecutableLinesAsync(connection, transaction, priceRevisionId, rollback, cancellationToken);
                var skipped = new List<PriceRevisionLineDto>();
                var appliedLineIds = new List<Guid>();

                foreach (var group in lines.GroupBy(line => new { line.TargetType, line.TargetId }))
                {
                    var isRuleTarget = group.Key.TargetType is TargetTypePricingRuleValue or TargetTypePricingRuleTier;

                    var groupSkips = isRuleTarget
                        ? await ExecutePricingRuleGroupAsync(connection, transaction, group.Key.TargetId, group.ToList(), rollback, appliedLineIds, cancellationToken)
                        : await ExecuteSimpleTargetsAsync(connection, transaction, group.Key.TargetType, group.ToList(), rollback, appliedLineIds, cancellationToken);

                    skipped.AddRange(groupSkips);
                }

                await MarkLinesAsync(connection, transaction, appliedLineIds, applied: !rollback, cancellationToken);
                await MarkSkippedLinesAsync(connection, transaction, skipped, cancellationToken);

                var status = rollback ? PriceRevisionStatusRolledBack : PriceRevisionStatusApplied;
                await StampExecutionAsync(connection, transaction, priceRevisionId, status, userId, rollback, cancellationToken);

                transaction.Commit();

                return new PriceRevisionExecutionResultDto
                {
                    PriceRevisionId = priceRevisionId,
                    Status = status,
                    AffectedLineCount = appliedLineIds.Count,
                    SkippedLineCount = skipped.Count,
                    SkippedLines = skipped
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static async Task<IReadOnlyList<PriceRevisionLineDto>> LoadExecutableLinesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid priceRevisionId,
            bool rollback,
            CancellationToken cancellationToken)
        {
            var sql = @"
SELECT Id, PriceRevisionId, TargetType, TargetId, TargetPath, ProductId, ProductName,
       TargetLabel, CurrencyCode, OldValue, NewValue, IsExcluded, IsApplied, SkipReason
FROM [Product].[PriceRevisionLines]
WHERE PriceRevisionId = @PriceRevisionId AND IsDeleted = 0
" + (rollback ? "  AND IsApplied = 1" : "  AND IsExcluded = 0 AND IsApplied = 0") + @"
ORDER BY TargetType, TargetId;";

            var lines = await connection.QueryAsync<PriceRevisionLineDto>(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));
            return lines.AsList();
        }

        /// <summary>
        /// Tek kolonluk hedefler. Beklenen değeri WHERE koşuluna koyduğumuz için,
        /// fiyat arada değişmişse UPDATE hiçbir satıra dokunmaz ve satır atlanmış sayılır.
        /// </summary>
        private static async Task<IReadOnlyList<PriceRevisionLineDto>> ExecuteSimpleTargetsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            int targetType,
            IReadOnlyList<PriceRevisionLineDto> lines,
            bool rollback,
            List<Guid> appliedLineIds,
            CancellationToken cancellationToken)
        {
            var (table, column) = ResolveSimpleTarget(targetType);
            var sql = $@"
UPDATE {table}
SET {column} = @TargetValue, UpdatedAt = @Now
WHERE Id = @TargetId AND IsDeleted = 0 AND {column} = @ExpectedValue;";

            var skipped = new List<PriceRevisionLineDto>();
            var now = DateTime.UtcNow;

            foreach (var line in lines)
            {
                var expected = rollback ? line.NewValue : line.OldValue;
                var target = rollback ? line.OldValue : line.NewValue;

                var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
                {
                    TargetId = line.TargetId,
                    TargetValue = target,
                    ExpectedValue = expected,
                    Now = now
                }, transaction, cancellationToken: cancellationToken));

                if (rows > 0)
                {
                    appliedLineIds.Add(line.Id);
                }
                else
                {
                    skipped.Add(line with { SkipReason = "Hedef fiyat arada değişmiş ya da silinmiş; satır atlandı." });
                }
            }

            return skipped;
        }

        private static (string Table, string Column) ResolveSimpleTarget(int targetType)
            => targetType switch
            {
                TargetTypeLicenseOfferingBasePrice => ("[Product].[ProductLicenseOfferings]", "BasePrice"),
                TargetTypeModuleOfferingPrice => ("[Product].[ProductModuleOfferingPrices]", "Price"),
                TargetTypeProductPrice => ("[Product].[ProductPrices]", "Amount"),
                TargetTypePriceListItem => ("[Product].[ProductPriceListItems]", "Amount"),
                _ => throw new InvalidOperationException($"Desteklenmeyen hedef türü: {targetType}")
            };

        /// <summary>
        /// Bir kuralın bütün satırları (taban değer ve kademeler) tek okuma-yazma turunda
        /// işlenir. Aksi hâlde her kademe kendi turunda JSON'un tamamını geri yazar ve
        /// bir öncekinin değişikliğini siler.
        /// </summary>
        private static async Task<IReadOnlyList<PriceRevisionLineDto>> ExecutePricingRuleGroupAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid pricingRuleId,
            IReadOnlyList<PriceRevisionLineDto> lines,
            bool rollback,
            List<Guid> appliedLineIds,
            CancellationToken cancellationToken)
        {
            const string selectSql = @"
SELECT PriceAdjustmentJson
FROM [Product].[ProductPricingRules]
WHERE Id = @PricingRuleId AND IsDeleted = 0;";

            var currentJson = await connection.QuerySingleOrDefaultAsync<string>(
                new CommandDefinition(selectSql, new { PricingRuleId = pricingRuleId }, transaction, cancellationToken: cancellationToken));

            if (currentJson is null || JsonNode.Parse(currentJson) is not JsonObject adjustment)
            {
                return lines
                    .Select(line => line with { SkipReason = "Fiyatlandırma kuralı bulunamadı ya da içeriği okunamadı." })
                    .ToList();
            }

            var skipped = new List<PriceRevisionLineDto>();
            var writable = new List<PriceRevisionLineDto>();

            foreach (var line in lines)
            {
                var expected = rollback ? line.NewValue : line.OldValue;

                if (!TryReadPathValue(adjustment, line.TargetPath, out var actual) || actual != expected)
                {
                    skipped.Add(line with { SkipReason = "Kuraldaki tutar arada değişmiş; satır atlandı." });
                    continue;
                }

                writable.Add(line);
            }

            if (writable.Count == 0)
            {
                return skipped;
            }

            foreach (var line in writable)
            {
                SetPathValue(adjustment, line.TargetPath, rollback ? line.OldValue : line.NewValue);
            }

            const string updateSql = @"
UPDATE [Product].[ProductPricingRules]
SET PriceAdjustmentJson = @PriceAdjustmentJson, UpdatedAt = @Now
WHERE Id = @PricingRuleId AND IsDeleted = 0 AND PriceAdjustmentJson = @CurrentJson;";

            var rows = await connection.ExecuteAsync(new CommandDefinition(updateSql, new
            {
                PricingRuleId = pricingRuleId,
                PriceAdjustmentJson = adjustment.ToJsonString(),
                CurrentJson = currentJson,
                Now = DateTime.UtcNow
            }, transaction, cancellationToken: cancellationToken));

            if (rows > 0)
            {
                appliedLineIds.AddRange(writable.Select(line => line.Id));
            }
            else
            {
                skipped.AddRange(writable.Select(line => line with { SkipReason = "Kural arada değişmiş; satır atlandı." }));
            }

            return skipped;
        }

        private static bool TryReadPathValue(JsonObject adjustment, string path, out decimal value)
        {
            value = 0;
            var tierMatch = TierPathPattern.Match(path);

            if (tierMatch.Success)
            {
                var index = int.Parse(tierMatch.Groups["index"].Value);
                return adjustment["tiers"] is JsonArray tiers
                    && index < tiers.Count
                    && tiers[index] is JsonObject tier
                    && TryReadDecimal(tier, tierMatch.Groups["field"].Value, out value);
            }

            var field = path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : path;
            return TryReadDecimal(adjustment, field, out value);
        }

        private static void SetPathValue(JsonObject adjustment, string path, decimal value)
        {
            var tierMatch = TierPathPattern.Match(path);

            if (tierMatch.Success)
            {
                var index = int.Parse(tierMatch.Groups["index"].Value);
                if (adjustment["tiers"] is JsonArray tiers && index < tiers.Count && tiers[index] is JsonObject tier)
                {
                    tier[tierMatch.Groups["field"].Value] = TrimScale(value);
                }

                return;
            }

            adjustment[path.StartsWith("$.", StringComparison.Ordinal) ? path[2..] : path] = TrimScale(value);
        }

        /// <summary>
        /// decimal ondalık basamak sayısını taşır: 0,24 ile 0,2400 aynı sayıdır ama JSON'a
        /// farklı yazılır. Değerler veritabanından <c>decimal(18,4)</c> olarak okunduğu için
        /// kırpma tam da JSON'a yazım anında yapılmalıdır; aksi hâlde geri alma, kural
        /// gövdesini başlangıçtakiyle aynı bırakmaz.
        /// </summary>
        private static decimal TrimScale(decimal value)
            => value / 1.000000000000000000000000000m;

        private static async Task MarkLinesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            IReadOnlyList<Guid> lineIds,
            bool applied,
            CancellationToken cancellationToken)
        {
            if (lineIds.Count == 0)
            {
                return;
            }

            const string sql = @"
UPDATE [Product].[PriceRevisionLines]
SET IsApplied = @IsApplied, SkipReason = NULL, UpdatedAt = @Now
WHERE Id IN @LineIds;";

            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                LineIds = lineIds,
                IsApplied = applied,
                Now = DateTime.UtcNow
            }, transaction, cancellationToken: cancellationToken));
        }

        private static async Task MarkSkippedLinesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            IReadOnlyList<PriceRevisionLineDto> skipped,
            CancellationToken cancellationToken)
        {
            if (skipped.Count == 0)
            {
                return;
            }

            const string sql = @"
UPDATE [Product].[PriceRevisionLines]
SET SkipReason = @SkipReason, UpdatedAt = @Now
WHERE Id = @Id;";

            var now = DateTime.UtcNow;
            var parameters = skipped.Select(line => new { line.Id, line.SkipReason, Now = now }).ToList();

            await connection.ExecuteAsync(
                new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task StampExecutionAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid priceRevisionId,
            int status,
            Guid? userId,
            bool rollback,
            CancellationToken cancellationToken)
        {
            var sql = rollback
                ? @"
UPDATE [Product].[PriceRevisions]
SET Status = @Status, RolledBackAt = @Now, RolledBackByUserId = @UserId, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;"
                : @"
UPDATE [Product].[PriceRevisions]
SET Status = @Status, AppliedAt = @Now, AppliedByUserId = @UserId, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PriceRevisionId = priceRevisionId,
                Status = status,
                UserId = userId,
                Now = DateTime.UtcNow
            }, transaction, cancellationToken: cancellationToken));
        }
    }
}
