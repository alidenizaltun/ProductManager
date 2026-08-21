using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Exceptions;
using System.Data;
using System.Text.Json.Nodes;

namespace ProductManagement.Repository.Concrete
{
    /// <summary>
    /// Ürün bağımsız fiyat şablonları. Şablon bir ürüne uygulandığında değerler
    /// ürünün kendi <c>ProductPricingRules</c> satırına kopyalanır; satıra kaynağın
    /// izi (<c>SourceTemplateId</c>, <c>SourceTemplateVersion</c>) yazılır. Fiyat motoru
    /// bu yüzden hiç değişmez, buna karşılık zam kapsamı izi takip ederek bulunabilir.
    /// </summary>
    public sealed partial class ProductOperationsRepository
    {
        private const int PricingTemplateKindPricingRule = 1;

        private const string PricingTemplateSelect = @"
SELECT t.Id, t.Code, t.Name, t.Description, t.TemplateKind,
       t.UnitDefinitionId, ud.Code AS UnitDefinitionCode, ud.Name AS UnitDefinitionName,
       t.CurrencyCode, t.PayloadJson, t.Version, t.IsActive, t.SortOrder,
       (
           SELECT COUNT(1)
           FROM [Product].[ProductPricingRules] r
           WHERE r.SourceTemplateId = t.Id AND r.IsDeleted = 0
       ) AS UsageCount,
       t.CreatedAt, t.UpdatedAt
FROM [Product].[PricingTemplates] t
LEFT JOIN [Product].[UnitDefinitions] ud ON ud.Id = t.UnitDefinitionId AND ud.IsDeleted = 0";

        public async Task<IReadOnlyList<PricingTemplateDto>> GetPricingTemplatesAsync(
            int? templateKind = null,
            Guid? unitDefinitionId = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var sql = PricingTemplateSelect + @"
WHERE t.IsDeleted = 0
  AND (@IncludeInactive = 1 OR t.IsActive = 1)
  AND (@TemplateKind IS NULL OR t.TemplateKind = @TemplateKind)
  AND (@UnitDefinitionId IS NULL OR t.UnitDefinitionId = @UnitDefinitionId)
ORDER BY t.SortOrder, t.Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<PricingTemplateDto>(
                new CommandDefinition(sql, new
                {
                    IncludeInactive = includeInactive,
                    TemplateKind = templateKind,
                    UnitDefinitionId = unitDefinitionId
                }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<PricingTemplateDto?> GetPricingTemplateByIdAsync(
            Guid pricingTemplateId,
            CancellationToken cancellationToken = default)
        {
            var sql = PricingTemplateSelect + @"
WHERE t.Id = @PricingTemplateId AND t.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<PricingTemplateDto>(
                new CommandDefinition(sql, new { PricingTemplateId = pricingTemplateId }, cancellationToken: cancellationToken));
        }

        public async Task<PricingTemplateDto> CreatePricingTemplateAsync(
            CreatePricingTemplateRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[PricingTemplates]
    (Id, Code, Name, Description, TemplateKind, UnitDefinitionId, CurrencyCode,
     PayloadJson, Version, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @Code, @Name, @Description, @TemplateKind, @UnitDefinitionId, @CurrencyCode,
     @PayloadJson, 1, @IsActive, @SortOrder, @Now, 0);";

            var payloadJson = ResolvePriceAdjustmentJson(request.PayloadJson, request.Payload);

            var id = await InsertWithGeneratedCodeAsync(
                request.Code,
                PricingTemplateCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var templateId = Guid.NewGuid();
                    await connection.ExecuteAsync(new CommandDefinition(sql, new
                    {
                        Id = templateId,
                        Code = code,
                        request.Name,
                        request.Description,
                        request.TemplateKind,
                        request.UnitDefinitionId,
                        request.CurrencyCode,
                        PayloadJson = payloadJson,
                        request.IsActive,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    }, transaction, cancellationToken: ct));
                    return templateId;
                },
                cancellationToken);

            return await GetPricingTemplateByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("PricingTemplate could not be loaded after insert.");
        }

        /// <summary>
        /// Payload değiştiyse <c>Version</c> artar. Sürüm, kopyalanan kuralların şablonun
        /// gerisinde kalıp kalmadığını göstermenin tek yoludur; bu yüzden yalnızca fiyat
        /// gövdesi değiştiğinde artırılır, ad ya da açıklama değişikliğinde değil.
        /// </summary>
        public async Task<bool> UpdatePricingTemplateAsync(
            Guid pricingTemplateId,
            UpdatePricingTemplateRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[PricingTemplates]
SET Code = @Code,
    Name = @Name,
    Description = @Description,
    UnitDefinitionId = @UnitDefinitionId,
    CurrencyCode = @CurrencyCode,
    Version = CASE WHEN PayloadJson = @PayloadJson THEN Version ELSE Version + 1 END,
    PayloadJson = @PayloadJson,
    IsActive = @IsActive,
    SortOrder = @SortOrder,
    UpdatedAt = @Now
WHERE Id = @PricingTemplateId AND IsDeleted = 0;";

            var payloadJson = ResolvePriceAdjustmentJson(request.PayloadJson, request.Payload);

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                PricingTemplateId = pricingTemplateId,
                request.Code,
                request.Name,
                request.Description,
                request.UnitDefinitionId,
                request.CurrencyCode,
                PayloadJson = payloadJson,
                request.IsActive,
                request.SortOrder,
                Now = DateTime.UtcNow
            }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<bool> DeletePricingTemplateAsync(
            Guid pricingTemplateId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[PricingTemplates]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @PricingTemplateId AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PricingTemplateId = pricingTemplateId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));
            return rows > 0;
        }

        public async Task<IReadOnlyList<PricingTemplateUsageDto>> GetPricingTemplateUsagesAsync(
            Guid pricingTemplateId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT r.Id AS PricingRuleId, r.Code AS PricingRuleCode, r.Name AS PricingRuleName,
       p.Id AS ProductId, p.ProductCode, p.Name AS ProductName,
       r.ProductLicenseOfferingId, o.Name AS LicenseOfferingName,
       r.SourceTemplateVersion, t.Version AS TemplateVersion, r.IsActive
FROM [Product].[ProductPricingRules] r
JOIN [Product].[PricingTemplates] t ON t.Id = r.SourceTemplateId
JOIN [Product].[Products] p ON p.Id = r.ProductId AND p.IsDeleted = 0
LEFT JOIN [Product].[ProductLicenseOfferings] o ON o.Id = r.ProductLicenseOfferingId AND o.IsDeleted = 0
WHERE r.SourceTemplateId = @PricingTemplateId AND r.IsDeleted = 0
ORDER BY p.Name, r.Priority;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<PricingTemplateUsageDto>(
                new CommandDefinition(sql, new { PricingTemplateId = pricingTemplateId }, cancellationToken: cancellationToken));
            return items.AsList();
        }

        public async Task<PricingTemplateDto> SavePricingRuleAsTemplateAsync(
            Guid pricingRuleId,
            SavePricingRuleAsTemplateRequestDto request,
            CancellationToken cancellationToken = default)
        {
            const string ruleSql = @"
SELECT TOP 1 r.Name, r.Description, r.PriceAdjustmentJson,
       pu.UnitDefinitionId, p.DefaultCurrencyCode
FROM [Product].[ProductPricingRules] r
JOIN [Product].[Products] p ON p.Id = r.ProductId
LEFT JOIN [Product].[ProductPricingRuleUnits] ru ON ru.ProductPricingRuleId = r.Id AND ru.IsDeleted = 0
LEFT JOIN [Product].[ProductUnits] pu ON pu.Id = ru.ProductUnitId AND pu.IsDeleted = 0
WHERE r.Id = @PricingRuleId AND r.IsDeleted = 0;";

            using var connection = CreateConnection();
            var rule = await connection.QuerySingleOrDefaultAsync<PricingRuleTemplateSource>(
                new CommandDefinition(ruleSql, new { PricingRuleId = pricingRuleId }, cancellationToken: cancellationToken));

            if (rule is null)
            {
                throw new NotFoundException("Fiyatlandırma kuralı bulunamadı.");
            }

            return await CreatePricingTemplateAsync(new CreatePricingTemplateRequestDto
            {
                Code = request.Code,
                Name = string.IsNullOrWhiteSpace(request.Name) ? rule.Name : request.Name,
                Description = request.Description ?? rule.Description,
                TemplateKind = PricingTemplateKindPricingRule,
                UnitDefinitionId = rule.UnitDefinitionId,
                CurrencyCode = rule.DefaultCurrencyCode,
                PayloadJson = rule.PriceAdjustmentJson,
                IsActive = request.IsActive
            }, cancellationToken);
        }

        /// <summary>
        /// Şablonu tek bir ürüne uygular. Birim çözümlemesi, kod çakışması ve kural
        /// eklemesi tek transaction içinde yapılır; hedef üründe şablonun birimi yoksa
        /// birim de burada oluşturulur.
        /// </summary>
        public async Task<ApplyPricingTemplateResultDto> ApplyPricingTemplateAsync(
            Guid pricingTemplateId,
            ApplyPricingTemplateRequestDto request,
            CancellationToken cancellationToken = default)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await ApplyPricingTemplateCoreAsync(
                    connection, transaction, pricingTemplateId, request, cancellationToken);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Toplu uygulama. Her ürün kendi transaction'ında işlenir: bir üründeki hata
        /// diğerlerini geri almaz, sonuç listesinde tek tek raporlanır.
        /// </summary>
        public async Task<IReadOnlyList<ApplyPricingTemplateResultDto>> ApplyPricingTemplateBulkAsync(
            Guid pricingTemplateId,
            ApplyPricingTemplateBulkRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var results = new List<ApplyPricingTemplateResultDto>(request.ProductIds.Count);

            foreach (var productId in request.ProductIds.Distinct())
            {
                var singleRequest = new ApplyPricingTemplateRequestDto
                {
                    ProductId = productId,
                    Priority = request.Priority,
                    IsActive = request.IsActive,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    OverrideValue = request.OverrideValue
                };

                try
                {
                    results.Add(await ApplyPricingTemplateAsync(pricingTemplateId, singleRequest, cancellationToken));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Tek bir ürünün hatası partinin geri kalanını düşürmemeli; sonuç
                    // listesi hem başarılıları hem başarısızları taşır.
                    results.Add(new ApplyPricingTemplateResultDto
                    {
                        ProductId = productId,
                        Succeeded = false,
                        Message = ex is BaseException ? ex.Message : "Beklenmedik bir hata oluştu."
                    });
                }
            }

            return results;
        }

        private async Task<ApplyPricingTemplateResultDto> ApplyPricingTemplateCoreAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid pricingTemplateId,
            ApplyPricingTemplateRequestDto request,
            CancellationToken cancellationToken)
        {
            const string templateSql = @"
SELECT Id, Code, Name, TemplateKind, UnitDefinitionId, CurrencyCode, PayloadJson, Version
FROM [Product].[PricingTemplates]
WHERE Id = @PricingTemplateId AND IsDeleted = 0;";

            var template = await connection.QuerySingleOrDefaultAsync<PricingTemplateApplySource>(
                new CommandDefinition(templateSql, new { PricingTemplateId = pricingTemplateId }, transaction, cancellationToken: cancellationToken))
                ?? throw new NotFoundException("Fiyat şablonu bulunamadı.");

            if (template.TemplateKind != PricingTemplateKindPricingRule)
            {
                throw new ValidationException(
                    "templateKind",
                    "Şu an yalnızca fiyatlandırma kuralı şablonları ürüne uygulanabilir.");
            }

            const string productSql = @"
SELECT Name FROM [Product].[Products] WHERE Id = @ProductId AND IsDeleted = 0;";

            var productName = await connection.QuerySingleOrDefaultAsync<string>(
                new CommandDefinition(productSql, new { request.ProductId }, transaction, cancellationToken: cancellationToken))
                ?? throw new NotFoundException("Ürün bulunamadı.");

            var (productUnitId, createdProductUnitId) = await ResolveTemplateProductUnitAsync(
                connection, transaction, request.ProductId, template.UnitDefinitionId, cancellationToken);

            var ruleCode = await ResolveAvailablePricingRuleCodeAsync(
                connection, transaction, request.ProductId, template.Code, cancellationToken);

            var payloadJson = ApplyOverrideValue(template.PayloadJson, request.OverrideValue, template.Code);

            const string insertSql = @"
INSERT INTO [Product].[ProductPricingRules]
(
    Id, ProductId, Code, Name, Description, PriceAdjustmentJson,
    Priority, IsActive, ValidFrom, ValidTo, SalesChannel, CustomerGroupCode,
    ProductVariantId, ProductLicenseOfferingId, SourceTemplateId, SourceTemplateVersion,
    CreatedAt, IsDeleted
)
VALUES
(
    @Id, @ProductId, @Code, @Name, NULL, @PriceAdjustmentJson,
    @Priority, @IsActive, @ValidFrom, @ValidTo, NULL, NULL,
    @ProductVariantId, @ProductLicenseOfferingId, @SourceTemplateId, @SourceTemplateVersion,
    @Now, 0
);";

            var ruleId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var linkedOfferingCount = 0;

            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = ruleId,
                request.ProductId,
                Code = ruleCode,
                template.Name,
                PriceAdjustmentJson = payloadJson,
                request.Priority,
                request.IsActive,
                request.ValidFrom,
                request.ValidTo,
                request.ProductVariantId,
                ProductLicenseOfferingId = request.LicenseOfferingId,
                SourceTemplateId = template.Id,
                SourceTemplateVersion = template.Version,
                Now = now
            }, transaction, cancellationToken: cancellationToken));

            if (productUnitId.HasValue)
            {
                // Birim üç yere birden bağlanır: ürüne (yukarıda), kurala ve satış planlarına.
                // Kural atamasi "bu kural bu birim icin gecerlidir" kapsamini ifade eder;
                // kuralin miktari yine priceAdjustment.unit.field (ör. feature.smsCount)
                // uzerinden gelir, bu atamadan degil.
                await InsertPricingRuleUnitAssignmentsAsync(
                    connection, transaction, ruleId, [productUnitId.Value], now, cancellationToken);

                linkedOfferingCount = await LinkUnitToOfferingsAsync(
                    connection,
                    transaction,
                    request.ProductId,
                    request.LicenseOfferingId,
                    productUnitId.Value,
                    now,
                    cancellationToken);
            }

            return new ApplyPricingTemplateResultDto
            {
                ProductId = request.ProductId,
                ProductName = productName,
                Succeeded = true,
                PricingRuleId = ruleId,
                PricingRuleCode = ruleCode,
                CreatedProductUnitId = createdProductUnitId,
                LinkedProductUnitId = productUnitId,
                LinkedOfferingCount = linkedOfferingCount
            };
        }

        /// <summary>
        /// Şablonun birimini hedef üründe bulur; yoksa birim tanımından oluşturur.
        /// Şablon bir birime bağlı değilse (<c>UnitDefinitionId</c> boş) hiçbir şey yapmaz.
        /// </summary>
        private static async Task<(Guid? ProductUnitId, Guid? CreatedProductUnitId)> ResolveTemplateProductUnitAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            Guid? unitDefinitionId,
            CancellationToken cancellationToken)
        {
            if (!unitDefinitionId.HasValue)
            {
                return (null, null);
            }

            const string existingSql = @"
SELECT TOP 1 Id
FROM [Product].[ProductUnits]
WHERE ProductId = @ProductId AND UnitDefinitionId = @UnitDefinitionId AND IsDeleted = 0
ORDER BY CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, CASE WHEN IsDefault = 1 THEN 0 ELSE 1 END, SortOrder;";

            var existingId = await connection.QuerySingleOrDefaultAsync<Guid?>(
                new CommandDefinition(existingSql, new { ProductId = productId, UnitDefinitionId = unitDefinitionId },
                    transaction, cancellationToken: cancellationToken));

            if (existingId.HasValue)
            {
                return (existingId, null);
            }

            const string definitionSql = @"
SELECT Code, Name, Description
FROM [Product].[UnitDefinitions]
WHERE Id = @UnitDefinitionId AND IsDeleted = 0;";

            var definition = await connection.QuerySingleOrDefaultAsync<UnitDefinitionSource>(
                new CommandDefinition(definitionSql, new { UnitDefinitionId = unitDefinitionId },
                    transaction, cancellationToken: cancellationToken))
                ?? throw new NotFoundException("Şablonun bağlı olduğu birim tanımı bulunamadı.");

            var unitCode = await ResolveAvailableProductUnitCodeAsync(
                connection, transaction, productId, definition.Code, cancellationToken);

            const string insertSql = @"
INSERT INTO [Product].[ProductUnits]
    (Id, ProductId, UnitDefinitionId, Code, Name, Description, Role, IsDefault, IsActive, SortOrder, CreatedAt, IsDeleted)
VALUES
    (@Id, @ProductId, @UnitDefinitionId, @Code, @Name, @Description, 1, 0, 1, 0, @Now, 0);";

            var newUnitId = Guid.NewGuid();
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = newUnitId,
                ProductId = productId,
                UnitDefinitionId = unitDefinitionId,
                Code = unitCode,
                definition.Name,
                definition.Description,
                Now = DateTime.UtcNow
            }, transaction, cancellationToken: cancellationToken));

            return (newUnitId, newUnitId);
        }

        /// <summary>
        /// Birimi ilgili satış planlarına bağlar. Plan verilmişse yalnızca ona, verilmemişse
        /// ürünün tüm aktif planlarına: kural plan kısıtı olmadan eklendiğinde her planda
        /// geçerli olacağı için birim de her planda görünmelidir.
        /// </summary>
        private static async Task<int> LinkUnitToOfferingsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            Guid? licenseOfferingId,
            Guid productUnitId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            const string offeringSql = @"
SELECT Id
FROM [Product].[ProductLicenseOfferings]
WHERE ProductId = @ProductId AND IsDeleted = 0
  AND (
        Id = @LicenseOfferingId
     OR (@LicenseOfferingId IS NULL AND IsActive = 1)
      );";

            var offeringIds = (await connection.QueryAsync<Guid>(
                new CommandDefinition(offeringSql, new { ProductId = productId, LicenseOfferingId = licenseOfferingId },
                    transaction, cancellationToken: cancellationToken))).AsList();

            foreach (var offeringId in offeringIds)
            {
                await MaterializeImplicitOfferingUnitsAsync(connection, transaction, productId, offeringId, now, cancellationToken);
                await EnsureLicenseOfferingUnitAsync(connection, transaction, offeringId, productUnitId, now, cancellationToken);
            }

            return offeringIds.Count;
        }

        /// <summary>
        /// Hiç birim atanmamış bir plan, fiyat motorunda ürünün varsayılan aktif birimlerini
        /// örtük olarak kullanır. Plana ilk birimi eklemek bu yedeği devre dışı bırakır ve
        /// örtük gelen birimi (ör. Kullanıcı) sessizce düşürür; bu yüzden ilk eklemeden önce
        /// örtük birimler kalıcı hâle getirilir.
        /// </summary>
        private static async Task MaterializeImplicitOfferingUnitsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            Guid offeringId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            const string sql = @"
INSERT INTO [Product].[ProductLicenseOfferingUnits]
    (Id, ProductLicenseOfferingId, ProductUnitId, CreatedAt, IsDeleted)
SELECT NEWID(), @OfferingId, pu.Id, @Now, 0
FROM [Product].[ProductUnits] pu
WHERE pu.ProductId = @ProductId AND pu.IsDeleted = 0 AND pu.IsActive = 1 AND pu.IsDefault = 1
  AND NOT EXISTS (
        SELECT 1 FROM [Product].[ProductLicenseOfferingUnits] existing
        WHERE existing.ProductLicenseOfferingId = @OfferingId);";

            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                OfferingId = offeringId,
                ProductId = productId,
                Now = now
            }, transaction, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Birimi satış planına bağlar. <c>IX_ProductLicenseOfferingUnits_Offering_Unit</c>
        /// benzersiz olduğu için bağlantı zaten varsa hiçbir şey yapılmaz.
        /// </summary>
        private static async Task EnsureLicenseOfferingUnitAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid licenseOfferingId,
            Guid productUnitId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            const string sql = @"
INSERT INTO [Product].[ProductLicenseOfferingUnits]
    (Id, ProductLicenseOfferingId, ProductUnitId, CreatedAt, IsDeleted)
SELECT @Id, @ProductLicenseOfferingId, @ProductUnitId, @Now, 0
WHERE NOT EXISTS (
    SELECT 1 FROM [Product].[ProductLicenseOfferingUnits]
    WHERE ProductLicenseOfferingId = @ProductLicenseOfferingId AND ProductUnitId = @ProductUnitId
);";

            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = Guid.NewGuid(),
                ProductLicenseOfferingId = licenseOfferingId,
                ProductUnitId = productUnitId,
                Now = now
            }, transaction, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// <c>IX_ProductPricingRules_ProductId_Code</c> benzersiz olduğu için aynı şablon
        /// aynı ürüne ikinci kez uygulandığında koda sıra eki verilir (TPL-000004-2).
        /// </summary>
        private static Task<string> ResolveAvailablePricingRuleCodeAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            string baseCode,
            CancellationToken cancellationToken)
            => ResolveAvailableCodeAsync(
                connection,
                transaction,
                @"SELECT Code FROM [Product].[ProductPricingRules]
                  WHERE ProductId = @ProductId AND IsDeleted = 0 AND Code LIKE @Pattern;",
                productId,
                baseCode,
                cancellationToken);

        private static Task<string> ResolveAvailableProductUnitCodeAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            string baseCode,
            CancellationToken cancellationToken)
            => ResolveAvailableCodeAsync(
                connection,
                transaction,
                @"SELECT Code FROM [Product].[ProductUnits]
                  WHERE ProductId = @ProductId AND IsDeleted = 0 AND Code LIKE @Pattern;",
                productId,
                baseCode,
                cancellationToken);

        private static async Task<string> ResolveAvailableCodeAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string existingCodesSql,
            Guid productId,
            string baseCode,
            CancellationToken cancellationToken)
        {
            var taken = (await connection.QueryAsync<string>(
                new CommandDefinition(existingCodesSql, new
                {
                    ProductId = productId,
                    Pattern = $"{baseCode}%"
                }, transaction, cancellationToken: cancellationToken)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!taken.Contains(baseCode))
            {
                return baseCode;
            }

            for (var suffix = 2; suffix < 1000; suffix++)
            {
                var candidate = $"{baseCode}-{suffix}";
                if (!taken.Contains(candidate))
                {
                    return candidate;
                }
            }

            throw new ConflictException($"'{baseCode}' için kullanılabilir bir kod üretilemedi.");
        }

        /// <summary>
        /// Tek seferlik değer farkı. Kademeli (tiers) bir şablonda tek bir değeri
        /// değiştirmek anlamsız olacağı için bu durumda hata verilir.
        /// </summary>
        private static string ApplyOverrideValue(string payloadJson, decimal? overrideValue, string templateCode)
        {
            if (!overrideValue.HasValue)
            {
                return payloadJson;
            }

            if (JsonNode.Parse(payloadJson) is not JsonObject payload)
            {
                throw new ValidationException("payloadJson", $"Şablon içeriği okunamadı: {templateCode}");
            }

            if (payload["tiers"] is JsonArray { Count: > 0 })
            {
                throw new ValidationException(
                    "overrideValue",
                    "Kademeli şablonlarda tek değer geçersiz kılınamaz. Kademeleri düzenlemek için kuralı uyguladıktan sonra değiştirin.");
            }

            payload["value"] = overrideValue.Value;
            return payload.ToJsonString();
        }

        private sealed record PricingRuleTemplateSource
        {
            public string Name { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string PriceAdjustmentJson { get; init; } = string.Empty;
            public Guid? UnitDefinitionId { get; init; }
            public string DefaultCurrencyCode { get; init; } = "TRY";
        }

        private sealed record PricingTemplateApplySource
        {
            public Guid Id { get; init; }
            public string Code { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public int TemplateKind { get; init; }
            public Guid? UnitDefinitionId { get; init; }
            public string CurrencyCode { get; init; } = "TRY";
            public string PayloadJson { get; init; } = string.Empty;
            public int Version { get; init; }
        }

        private sealed record UnitDefinitionSource
        {
            public string Code { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public string? Description { get; init; }
        }
    }
}
