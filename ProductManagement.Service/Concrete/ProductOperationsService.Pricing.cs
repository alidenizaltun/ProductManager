using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.Service.Concrete
{
    /// <summary>
    /// Fiyat şablonları ve zam revizyonları. Revizyonun durum geçişleri burada
    /// denetlenir: bir belge yalnızca doğru durumdayken önizlenebilir, onaya
    /// gönderilebilir, onaylanabilir, uygulanabilir ve geri alınabilir.
    /// </summary>
    public sealed partial class ProductOperationsService
    {
        private const int StatusDraft = 1;
        private const int StatusPreviewed = 2;
        private const int StatusPendingApproval = 3;
        private const int StatusApproved = 4;
        private const int StatusApplied = 5;
        private const int StatusRolledBack = 6;
        private const int StatusRejected = 7;
        private const int StatusCancelled = 8;

        // ─── Fiyat şablonları ────────────────────────────────────────────────────────

        public Task<IReadOnlyList<PricingTemplateDto>> GetPricingTemplatesAsync(
            int? templateKind = null,
            Guid? unitDefinitionId = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
            => _repository.GetPricingTemplatesAsync(templateKind, unitDefinitionId, includeInactive, cancellationToken);

        public Task<PricingTemplateDto?> GetPricingTemplateByIdAsync(Guid pricingTemplateId, CancellationToken cancellationToken = default)
            => _repository.GetPricingTemplateByIdAsync(pricingTemplateId, cancellationToken);

        public Task<PricingTemplateDto> CreatePricingTemplateAsync(CreatePricingTemplateRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePricingTemplateAsync(request, cancellationToken));

        public Task<bool> UpdatePricingTemplateAsync(Guid pricingTemplateId, UpdatePricingTemplateRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdatePricingTemplateAsync(pricingTemplateId, request, cancellationToken));

        public Task<bool> DeletePricingTemplateAsync(Guid pricingTemplateId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePricingTemplateAsync(pricingTemplateId, cancellationToken));

        public Task<IReadOnlyList<PricingTemplateUsageDto>> GetPricingTemplateUsagesAsync(Guid pricingTemplateId, CancellationToken cancellationToken = default)
            => _repository.GetPricingTemplateUsagesAsync(pricingTemplateId, cancellationToken);

        public Task<PricingTemplateDto> SavePricingRuleAsTemplateAsync(Guid pricingRuleId, SavePricingRuleAsTemplateRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.SavePricingRuleAsTemplateAsync(pricingRuleId, request, cancellationToken));

        public Task<ApplyPricingTemplateResultDto> ApplyPricingTemplateAsync(Guid pricingTemplateId, ApplyPricingTemplateRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.ApplyPricingTemplateAsync(pricingTemplateId, request, cancellationToken));

        public Task<IReadOnlyList<ApplyPricingTemplateResultDto>> ApplyPricingTemplateBulkAsync(Guid pricingTemplateId, ApplyPricingTemplateBulkRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.ApplyPricingTemplateBulkAsync(pricingTemplateId, request, cancellationToken));

        // ─── Zam revizyonları ────────────────────────────────────────────────────────

        public Task<IReadOnlyList<PriceRevisionDto>> GetPriceRevisionsAsync(int? status = null, CancellationToken cancellationToken = default)
            => _repository.GetPriceRevisionsAsync(status, cancellationToken);

        public Task<PriceRevisionDto?> GetPriceRevisionByIdAsync(Guid priceRevisionId, CancellationToken cancellationToken = default)
            => _repository.GetPriceRevisionByIdAsync(priceRevisionId, cancellationToken);

        public Task<PriceRevisionDto> CreatePriceRevisionAsync(CreatePriceRevisionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePriceRevisionAsync(request, cancellationToken));

        public async Task<bool> UpdatePriceRevisionAsync(Guid priceRevisionId, UpdatePriceRevisionRequestDto request, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "düzenlenemez", cancellationToken, StatusDraft, StatusPreviewed, StatusRejected);
            return await ExecuteWithSqlMapping(() => _repository.UpdatePriceRevisionAsync(priceRevisionId, request, cancellationToken));
        }

        public async Task<bool> DeletePriceRevisionAsync(Guid priceRevisionId, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "silinemez", cancellationToken,
                StatusDraft, StatusPreviewed, StatusPendingApproval, StatusRejected, StatusCancelled);
            return await ExecuteWithSqlMapping(() => _repository.DeletePriceRevisionAsync(priceRevisionId, cancellationToken));
        }

        public async Task<PriceRevisionScopeDto> CreatePriceRevisionScopeAsync(Guid priceRevisionId, CreatePriceRevisionScopeRequestDto request, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "kapsamı değiştirilemez", cancellationToken, StatusDraft, StatusPreviewed, StatusRejected);
            return await ExecuteWithSqlMapping(() => _repository.CreatePriceRevisionScopeAsync(priceRevisionId, request, cancellationToken));
        }

        public async Task<bool> DeletePriceRevisionScopeAsync(Guid priceRevisionId, Guid scopeId, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "kapsamı değiştirilemez", cancellationToken, StatusDraft, StatusPreviewed, StatusRejected);
            return await ExecuteWithSqlMapping(() => _repository.DeletePriceRevisionScopeAsync(priceRevisionId, scopeId, cancellationToken));
        }

        public async Task<PriceRevisionSummaryDto> PreviewPriceRevisionAsync(Guid priceRevisionId, CancellationToken cancellationToken = default)
        {
            var revision = await EnsureRevisionStatusAsync(priceRevisionId, "önizlenemez", cancellationToken, StatusDraft, StatusPreviewed, StatusRejected);

            if (revision.Scopes.Count == 0)
            {
                throw new ValidationException("scopes", "Önizleme için en az bir kapsam satırı gerekir.");
            }

            return await ExecuteWithSqlMapping(() => _repository.PreviewPriceRevisionAsync(priceRevisionId, cancellationToken));
        }

        public Task<PriceRevisionLinePageDto> GetPriceRevisionLinesAsync(Guid priceRevisionId, PriceRevisionLineFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetPriceRevisionLinesAsync(priceRevisionId, filter, cancellationToken);

        public async Task<bool> UpdatePriceRevisionLineAsync(Guid priceRevisionId, Guid lineId, UpdatePriceRevisionLineRequestDto request, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "satırları düzenlenemez", cancellationToken, StatusPreviewed, StatusPendingApproval, StatusRejected);
            return await ExecuteWithSqlMapping(() => _repository.UpdatePriceRevisionLineAsync(priceRevisionId, lineId, request, cancellationToken));
        }

        /// <summary>Onaya gönderir. Satırı olmayan bir revizyon onaya gidemez.</summary>
        public async Task<bool> SubmitPriceRevisionAsync(Guid priceRevisionId, Guid? userId, CancellationToken cancellationToken = default)
        {
            var revision = await EnsureRevisionStatusAsync(priceRevisionId, "onaya gönderilemez", cancellationToken, StatusPreviewed, StatusRejected);

            if (revision.Summary is null || revision.Summary.LineCount == revision.Summary.ExcludedLineCount)
            {
                throw new ValidationException("lines", "Onaya göndermek için en az bir dahil edilmiş satır gerekir.");
            }

            return await ExecuteWithSqlMapping(() =>
                _repository.SetPriceRevisionStatusAsync(priceRevisionId, StatusPendingApproval, userId, null, cancellationToken));
        }

        public async Task<bool> ApprovePriceRevisionAsync(Guid priceRevisionId, Guid? userId, string? note, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "onaylanamaz", cancellationToken, StatusPendingApproval);
            return await ExecuteWithSqlMapping(() =>
                _repository.SetPriceRevisionStatusAsync(priceRevisionId, StatusApproved, userId, note, cancellationToken));
        }

        public async Task<bool> RejectPriceRevisionAsync(Guid priceRevisionId, Guid? userId, string note, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "reddedilemez", cancellationToken, StatusPendingApproval);
            return await ExecuteWithSqlMapping(() =>
                _repository.SetPriceRevisionStatusAsync(priceRevisionId, StatusRejected, userId, note, cancellationToken));
        }

        public async Task<bool> CancelPriceRevisionAsync(Guid priceRevisionId, Guid? userId, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "iptal edilemez", cancellationToken,
                StatusDraft, StatusPreviewed, StatusPendingApproval, StatusRejected);
            return await ExecuteWithSqlMapping(() =>
                _repository.SetPriceRevisionStatusAsync(priceRevisionId, StatusCancelled, userId, null, cancellationToken));
        }

        /// <summary>
        /// Onaylı revizyonu uygular. İleri tarihli revizyonlar erken uygulanamaz;
        /// belge <c>EffectiveDate</c> gelene kadar onaylı durumda bekler.
        /// </summary>
        public async Task<PriceRevisionExecutionResultDto> ApplyPriceRevisionAsync(Guid priceRevisionId, Guid? userId, CancellationToken cancellationToken = default)
        {
            var revision = await EnsureRevisionStatusAsync(priceRevisionId, "uygulanamaz", cancellationToken, StatusApproved);

            if (revision.EffectiveDate is { } effectiveDate && effectiveDate > DateTime.UtcNow)
            {
                throw new ValidationException(
                    "effectiveDate",
                    $"Revizyon {effectiveDate:dd.MM.yyyy} tarihinden önce uygulanamaz.");
            }

            return await ExecuteWithSqlMapping(() => _repository.ApplyPriceRevisionAsync(priceRevisionId, userId, cancellationToken));
        }

        public async Task<PriceRevisionExecutionResultDto> RollbackPriceRevisionAsync(Guid priceRevisionId, Guid? userId, CancellationToken cancellationToken = default)
        {
            await EnsureRevisionStatusAsync(priceRevisionId, "geri alınamaz", cancellationToken, StatusApplied);
            return await ExecuteWithSqlMapping(() => _repository.RollbackPriceRevisionAsync(priceRevisionId, userId, cancellationToken));
        }

        private async Task<PriceRevisionDto> EnsureRevisionStatusAsync(
            Guid priceRevisionId,
            string action,
            CancellationToken cancellationToken,
            params int[] allowedStatuses)
        {
            var revision = await _repository.GetPriceRevisionByIdAsync(priceRevisionId, cancellationToken)
                ?? throw new NotFoundException("Fiyat revizyonu bulunamadı.");

            if (!allowedStatuses.Contains(revision.Status))
            {
                throw new ConflictException($"'{DescribeStatus(revision.Status)}' durumundaki bir revizyon {action}.");
            }

            return revision;
        }

        private static string DescribeStatus(int status) => status switch
        {
            StatusDraft => "Taslak",
            StatusPreviewed => "Önizlendi",
            StatusPendingApproval => "Onay bekliyor",
            StatusApproved => "Onaylandı",
            StatusApplied => "Uygulandı",
            StatusRolledBack => "Geri alındı",
            StatusRejected => "Reddedildi",
            StatusCancelled => "İptal edildi",
            _ => "Bilinmiyor"
        };
    }
}
