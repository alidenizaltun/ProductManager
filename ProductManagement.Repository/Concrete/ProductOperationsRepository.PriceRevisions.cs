using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using System.Data;
using System.Text;

namespace ProductManagement.Repository.Concrete
{
    /// <summary>
    /// Zam (fiyat revizyonu) belgesinin veri işlemleri. Durum geçişlerinin hangi
    /// sırada yapılabileceği servis katmanında denetlenir; buradaki metotlar yalnızca
    /// veriyi okur ve yazar.
    /// </summary>
    public sealed partial class ProductOperationsRepository
    {
        private const string PriceRevisionSelect = @"
SELECT Id, Code, Name, Description, AdjustmentType, Value, RoundingMode, RoundingStep,
       CurrencyCode, Status, EffectiveDate,
       SubmittedAt, SubmittedByUserId, ApprovedAt, ApprovedByUserId, ApprovalNote,
       AppliedAt, AppliedByUserId, RolledBackAt, RolledBackByUserId,
       CreatedAt, UpdatedAt
FROM [Product].[PriceRevisions]";

        public async Task<IReadOnlyList<PriceRevisionDto>> GetPriceRevisionsAsync(
            int? status = null,
            CancellationToken cancellationToken = default)
        {
            var sql = PriceRevisionSelect + @"
WHERE IsDeleted = 0 AND (@Status IS NULL OR Status = @Status)
ORDER BY CreatedAt DESC;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<PriceRevisionDto>(
                new CommandDefinition(sql, new { Status = status }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<PriceRevisionDto?> GetPriceRevisionByIdAsync(
            Guid priceRevisionId,
            CancellationToken cancellationToken = default)
        {
            var sql = PriceRevisionSelect + @"
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var revision = await connection.QuerySingleOrDefaultAsync<PriceRevisionDto>(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, cancellationToken: cancellationToken));

            if (revision is null)
            {
                return null;
            }

            var scopes = await LoadPriceRevisionScopesAsync(connection, null, priceRevisionId, cancellationToken);
            var summary = await LoadPriceRevisionSummaryAsync(connection, null, priceRevisionId, cancellationToken);
            return revision with { Scopes = scopes, Summary = summary };
        }

        public async Task<PriceRevisionDto> CreatePriceRevisionAsync(
            CreatePriceRevisionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[PriceRevisions]
    (Id, Code, Name, Description, AdjustmentType, Value, RoundingMode, RoundingStep,
     CurrencyCode, Status, EffectiveDate, CreatedAt, IsDeleted)
VALUES
    (@Id, @Code, @Name, @Description, @AdjustmentType, @Value, @RoundingMode, @RoundingStep,
     @CurrencyCode, @Status, @EffectiveDate, @Now, 0);";

            var id = await InsertWithGeneratedCodeAsync(
                request.Code,
                PriceRevisionCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var revisionId = Guid.NewGuid();
                    await connection.ExecuteAsync(new CommandDefinition(sql, new
                    {
                        Id = revisionId,
                        Code = code,
                        request.Name,
                        request.Description,
                        request.AdjustmentType,
                        request.Value,
                        request.RoundingMode,
                        request.RoundingStep,
                        request.CurrencyCode,
                        Status = (int)PriceRevisionStatusDraft,
                        request.EffectiveDate,
                        Now = DateTime.UtcNow
                    }, transaction, cancellationToken: ct));
                    return revisionId;
                },
                cancellationToken);

            return await GetPriceRevisionByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("PriceRevision could not be loaded after insert.");
        }

        /// <summary>
        /// Revizyon başlığını günceller ve önizlemeyi geçersiz kılar: oran ya da yuvarlama
        /// değiştiğinde eski satırlar artık doğru olmadığı için silinir, durum taslağa döner.
        /// </summary>
        public async Task<bool> UpdatePriceRevisionAsync(
            Guid priceRevisionId,
            UpdatePriceRevisionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
UPDATE [Product].[PriceRevisions]
SET Code = @Code, Name = @Name, Description = @Description,
    AdjustmentType = @AdjustmentType, Value = @Value,
    RoundingMode = @RoundingMode, RoundingStep = @RoundingStep,
    CurrencyCode = @CurrencyCode, EffectiveDate = @EffectiveDate,
    Status = @Status, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            const string clearLinesSql = @"
DELETE FROM [Product].[PriceRevisionLines] WHERE PriceRevisionId = @PriceRevisionId;";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var rows = await connection.ExecuteAsync(new CommandDefinition(updateSql, new
                {
                    PriceRevisionId = priceRevisionId,
                    request.Code,
                    request.Name,
                    request.Description,
                    request.AdjustmentType,
                    request.Value,
                    request.RoundingMode,
                    request.RoundingStep,
                    request.CurrencyCode,
                    request.EffectiveDate,
                    Status = (int)PriceRevisionStatusDraft,
                    Now = DateTime.UtcNow
                }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(new CommandDefinition(
                    clearLinesSql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));

                transaction.Commit();
                return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeletePriceRevisionAsync(
            Guid priceRevisionId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[PriceRevisions]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        // ─── Kapsam ──────────────────────────────────────────────────────────────────

        public async Task<PriceRevisionScopeDto> CreatePriceRevisionScopeAsync(
            Guid priceRevisionId,
            CreatePriceRevisionScopeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string insertSql = @"
INSERT INTO [Product].[PriceRevisionScopes]
    (Id, PriceRevisionId, ScopeType, TargetId, TargetValue, IsExclude, CreatedAt, IsDeleted)
VALUES
    (@Id, @PriceRevisionId, @ScopeType, @TargetId, @TargetValue, @IsExclude, @Now, 0);";

            const string clearLinesSql = @"
DELETE FROM [Product].[PriceRevisionLines] WHERE PriceRevisionId = @PriceRevisionId;

UPDATE [Product].[PriceRevisions]
SET Status = @Status, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(new CommandDefinition(insertSql, new
                {
                    Id = id,
                    PriceRevisionId = priceRevisionId,
                    request.ScopeType,
                    request.TargetId,
                    request.TargetValue,
                    request.IsExclude,
                    Now = now
                }, transaction, cancellationToken: cancellationToken));

                await connection.ExecuteAsync(new CommandDefinition(clearLinesSql, new
                {
                    PriceRevisionId = priceRevisionId,
                    Status = (int)PriceRevisionStatusDraft,
                    Now = now
                }, transaction, cancellationToken: cancellationToken));

                var scopes = await LoadPriceRevisionScopesAsync(connection, transaction, priceRevisionId, cancellationToken);
                transaction.Commit();
                return scopes.First(scope => scope.Id == id);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeletePriceRevisionScopeAsync(
            Guid priceRevisionId,
            Guid scopeId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
DELETE FROM [Product].[PriceRevisionScopes]
WHERE Id = @ScopeId AND PriceRevisionId = @PriceRevisionId;

DELETE FROM [Product].[PriceRevisionLines] WHERE PriceRevisionId = @PriceRevisionId;

UPDATE [Product].[PriceRevisions]
SET Status = @Status, UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PriceRevisionId = priceRevisionId,
                ScopeId = scopeId,
                Status = (int)PriceRevisionStatusDraft,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        /// <summary>Kapsam satırlarını, ekranda gösterilecek hedef adlarıyla birlikte okur.</summary>
        private static async Task<IReadOnlyList<PriceRevisionScopeDto>> LoadPriceRevisionScopesAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid priceRevisionId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT s.Id, s.PriceRevisionId, s.ScopeType, s.TargetId, s.TargetValue, s.IsExclude,
       COALESCE(p.Name, c.Name, t.Name, ud.Name, o.Name, pl.Name, reg.Name) AS TargetName
FROM [Product].[PriceRevisionScopes] s
LEFT JOIN [Product].[Products] p ON s.ScopeType = 1 AND p.Id = s.TargetId
LEFT JOIN [Product].[ProductCategories] c ON s.ScopeType = 2 AND c.Id = s.TargetId
LEFT JOIN [Product].[PricingTemplates] t ON s.ScopeType = 3 AND t.Id = s.TargetId
LEFT JOIN [Product].[UnitDefinitions] ud ON s.ScopeType = 4 AND ud.Id = s.TargetId
LEFT JOIN [Product].[ProductLicenseOfferings] o ON s.ScopeType = 5 AND o.Id = s.TargetId
LEFT JOIN [Product].[ProductPriceLists] pl ON s.ScopeType = 6 AND pl.Id = s.TargetId
LEFT JOIN [Product].[Regions] reg ON s.ScopeType = 8 AND reg.Id = s.TargetId
WHERE s.PriceRevisionId = @PriceRevisionId AND s.IsDeleted = 0
ORDER BY s.IsExclude, s.ScopeType;";

            var items = await connection.QueryAsync<PriceRevisionScopeDto>(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));
            return items.AsList();
        }

        // ─── Satırlar ────────────────────────────────────────────────────────────────

        public async Task<PriceRevisionLinePageDto> GetPriceRevisionLinesAsync(
            Guid priceRevisionId,
            PriceRevisionLineFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var where = new StringBuilder(@"
WHERE PriceRevisionId = @PriceRevisionId AND IsDeleted = 0");

            if (filter.TargetType.HasValue)
            {
                where.Append(" AND TargetType = @TargetType");
            }

            if (filter.ProductId.HasValue)
            {
                where.Append(" AND ProductId = @ProductId");
            }

            if (filter.IsExcluded.HasValue)
            {
                where.Append(" AND IsExcluded = @IsExcluded");
            }

            var take = Math.Clamp(filter.Take <= 0 ? DefaultTake : filter.Take, 1, MaxTake);

            var sql = $@"
SELECT COUNT(1) FROM [Product].[PriceRevisionLines]{where};

SELECT Id, PriceRevisionId, TargetType, TargetId, TargetPath, ProductId, ProductName,
       TargetLabel, CurrencyCode, OldValue, NewValue, IsExcluded, IsApplied, SkipReason
FROM [Product].[PriceRevisionLines]{where}
ORDER BY ProductName, TargetType, TargetLabel
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new
            {
                PriceRevisionId = priceRevisionId,
                filter.TargetType,
                filter.ProductId,
                filter.IsExcluded,
                Skip = Math.Max(0, filter.Skip),
                Take = take
            }, cancellationToken: cancellationToken));

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<PriceRevisionLineDto>()).AsList();

            return new PriceRevisionLinePageDto { Items = items, TotalCount = totalCount };
        }

        public async Task<bool> UpdatePriceRevisionLineAsync(
            Guid priceRevisionId,
            Guid lineId,
            UpdatePriceRevisionLineRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[PriceRevisionLines]
SET IsExcluded = COALESCE(@IsExcluded, IsExcluded),
    NewValue = COALESCE(@NewValue, NewValue),
    UpdatedAt = @Now
WHERE Id = @LineId AND PriceRevisionId = @PriceRevisionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PriceRevisionId = priceRevisionId,
                LineId = lineId,
                request.IsExcluded,
                request.NewValue,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        private static async Task<PriceRevisionSummaryDto> LoadPriceRevisionSummaryAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            Guid priceRevisionId,
            CancellationToken cancellationToken)
        {
            const string sql = @"
SELECT COUNT(1) AS LineCount,
       SUM(CASE WHEN IsExcluded = 1 THEN 1 ELSE 0 END) AS ExcludedLineCount,
       COUNT(DISTINCT ProductId) AS ProductCount,
       ISNULL(SUM(CASE WHEN IsExcluded = 1 THEN 0 ELSE OldValue END), 0) AS TotalOldValue,
       ISNULL(SUM(CASE WHEN IsExcluded = 1 THEN 0 ELSE NewValue END), 0) AS TotalNewValue
FROM [Product].[PriceRevisionLines]
WHERE PriceRevisionId = @PriceRevisionId AND IsDeleted = 0;

SELECT TargetType,
       COUNT(1) AS LineCount,
       ISNULL(SUM(CASE WHEN IsExcluded = 1 THEN 0 ELSE OldValue END), 0) AS TotalOldValue,
       ISNULL(SUM(CASE WHEN IsExcluded = 1 THEN 0 ELSE NewValue END), 0) AS TotalNewValue
FROM [Product].[PriceRevisionLines]
WHERE PriceRevisionId = @PriceRevisionId AND IsDeleted = 0
GROUP BY TargetType
ORDER BY TargetType;";

            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { PriceRevisionId = priceRevisionId }, transaction, cancellationToken: cancellationToken));

            var summary = await multi.ReadSingleAsync<PriceRevisionSummaryDto>();
            var breakdown = (await multi.ReadAsync<PriceRevisionTargetBreakdownDto>()).AsList();
            return summary with { Breakdown = breakdown };
        }

        // ─── Durum damgaları ─────────────────────────────────────────────────────────

        public async Task<bool> SetPriceRevisionStatusAsync(
            Guid priceRevisionId,
            int status,
            Guid? userId,
            string? note,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[PriceRevisions]
SET Status = @Status,
    SubmittedAt   = CASE WHEN @Status = 3 THEN @Now  ELSE SubmittedAt END,
    SubmittedByUserId = CASE WHEN @Status = 3 THEN @UserId ELSE SubmittedByUserId END,
    ApprovedAt    = CASE WHEN @Status = 4 THEN @Now  ELSE ApprovedAt END,
    ApprovedByUserId  = CASE WHEN @Status IN (4, 7) THEN @UserId ELSE ApprovedByUserId END,
    ApprovalNote  = CASE WHEN @Status IN (4, 7) THEN @Note ELSE ApprovalNote END,
    UpdatedAt = @Now
WHERE Id = @PriceRevisionId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PriceRevisionId = priceRevisionId,
                Status = status,
                UserId = userId,
                Note = note,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }
    }
}
