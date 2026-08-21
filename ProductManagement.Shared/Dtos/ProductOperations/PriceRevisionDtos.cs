namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record PriceRevisionDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int AdjustmentType { get; init; }
        public decimal Value { get; init; }
        public int RoundingMode { get; init; }
        public decimal? RoundingStep { get; init; }
        public string? CurrencyCode { get; init; }
        public int Status { get; init; }
        public DateTime? EffectiveDate { get; init; }
        public DateTime? SubmittedAt { get; init; }
        public Guid? SubmittedByUserId { get; init; }
        public DateTime? ApprovedAt { get; init; }
        public Guid? ApprovedByUserId { get; init; }
        public string? ApprovalNote { get; init; }
        public DateTime? AppliedAt { get; init; }
        public Guid? AppliedByUserId { get; init; }
        public DateTime? RolledBackAt { get; init; }
        public Guid? RolledBackByUserId { get; init; }
        public IReadOnlyList<PriceRevisionScopeDto> Scopes { get; init; } = [];
        public PriceRevisionSummaryDto? Summary { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreatePriceRevisionRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (ZAM-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int AdjustmentType { get; init; } = 1;
        public decimal Value { get; init; }
        public int RoundingMode { get; init; } = 1;
        public decimal? RoundingStep { get; init; }
        public string? CurrencyCode { get; init; }
        public DateTime? EffectiveDate { get; init; }
    }

    public sealed record UpdatePriceRevisionRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int AdjustmentType { get; init; } = 1;
        public decimal Value { get; init; }
        public int RoundingMode { get; init; } = 1;
        public decimal? RoundingStep { get; init; }
        public string? CurrencyCode { get; init; }
        public DateTime? EffectiveDate { get; init; }
    }

    public sealed record PriceRevisionScopeDto
    {
        public Guid Id { get; init; }
        public Guid PriceRevisionId { get; init; }
        public int ScopeType { get; init; }
        public Guid? TargetId { get; init; }
        public string? TargetValue { get; init; }
        /// <summary>Ekranda gösterilecek hedef adı (ürün adı, kategori adı, şablon adı...).</summary>
        public string? TargetName { get; init; }
        public bool IsExclude { get; init; }
    }

    public sealed record CreatePriceRevisionScopeRequestDto
    {
        public int ScopeType { get; init; }
        public Guid? TargetId { get; init; }
        public string? TargetValue { get; init; }
        public bool IsExclude { get; init; }
    }

    public sealed record PriceRevisionLineDto
    {
        public Guid Id { get; init; }
        public Guid PriceRevisionId { get; init; }
        public int TargetType { get; init; }
        public Guid TargetId { get; init; }
        public string TargetPath { get; init; } = string.Empty;
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string TargetLabel { get; init; } = string.Empty;
        public string CurrencyCode { get; init; } = "TRY";
        public decimal OldValue { get; init; }
        public decimal NewValue { get; init; }
        public decimal Difference => NewValue - OldValue;
        public bool IsExcluded { get; init; }
        public bool IsApplied { get; init; }
        public string? SkipReason { get; init; }
    }

    public sealed record PriceRevisionLineFilterDto
    {
        public int? TargetType { get; init; }
        public Guid? ProductId { get; init; }
        public bool? IsExcluded { get; init; }
        public int Skip { get; init; }
        public int Take { get; init; } = 100;
    }

    public sealed record PriceRevisionLinePageDto
    {
        public IReadOnlyList<PriceRevisionLineDto> Items { get; init; } = [];
        public int TotalCount { get; init; }
    }

    public sealed record UpdatePriceRevisionLineRequestDto
    {
        public bool? IsExcluded { get; init; }
        /// <summary>Önizlenen değeri elle düzeltmek için.</summary>
        public decimal? NewValue { get; init; }
    }

    public sealed record PriceRevisionSummaryDto
    {
        public int LineCount { get; init; }
        public int ExcludedLineCount { get; init; }
        public int ProductCount { get; init; }
        public decimal TotalOldValue { get; init; }
        public decimal TotalNewValue { get; init; }
        public decimal TotalDifference => TotalNewValue - TotalOldValue;
        public IReadOnlyList<PriceRevisionTargetBreakdownDto> Breakdown { get; init; } = [];
        /// <summary>Kapsama girdiği hâlde zam uygulanamayan kurallar (oran/indirim tipli olanlar).</summary>
        public IReadOnlyList<PriceRevisionSkippedRuleDto> SkippedRules { get; init; } = [];
    }

    public sealed record PriceRevisionTargetBreakdownDto
    {
        public int TargetType { get; init; }
        public int LineCount { get; init; }
        public decimal TotalOldValue { get; init; }
        public decimal TotalNewValue { get; init; }
    }

    public sealed record PriceRevisionSkippedRuleDto
    {
        public Guid PricingRuleId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string PricingRuleName { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }

    public sealed record ApprovePriceRevisionRequestDto
    {
        public string? Note { get; init; }
    }

    public sealed record RejectPriceRevisionRequestDto
    {
        public required string Note { get; init; }
    }

    /// <summary>Uygula / geri al sonucunda ne olduğunu bildirir.</summary>
    public sealed record PriceRevisionExecutionResultDto
    {
        public Guid PriceRevisionId { get; init; }
        public int Status { get; init; }
        public int AffectedLineCount { get; init; }
        public int SkippedLineCount { get; init; }
        public IReadOnlyList<PriceRevisionLineDto> SkippedLines { get; init; } = [];
    }
}
