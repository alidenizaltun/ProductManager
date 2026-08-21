using System.Text.Json;

namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record PricingTemplateDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int TemplateKind { get; init; }
        public Guid? UnitDefinitionId { get; init; }
        public string? UnitDefinitionCode { get; init; }
        public string? UnitDefinitionName { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public string PayloadJson { get; init; } = string.Empty;
        public int Version { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        /// <summary>Bu şablondan türetilmiş, silinmemiş kural sayısı.</summary>
        public int UsageCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreatePricingTemplateRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (TPL-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int TemplateKind { get; init; } = 1;
        public Guid? UnitDefinitionId { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        /// <summary>Kuralın priceAdjustment gövdesi. Nesne ya da serileştirilmiş metin olarak gönderilebilir.</summary>
        public string? PayloadJson { get; init; }
        public JsonElement? Payload { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdatePricingTemplateRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public Guid? UnitDefinitionId { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public string? PayloadJson { get; init; }
        public JsonElement? Payload { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    /// <summary>Şablonun tek bir ürüne uygulanması.</summary>
    public sealed record ApplyPricingTemplateRequestDto
    {
        public Guid ProductId { get; init; }
        /// <summary>Doluysa kural yalnızca bu satış planı için geçerli olur.</summary>
        public Guid? LicenseOfferingId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public int Priority { get; init; }
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        /// <summary>Doluysa şablon payload'ındaki tek seferlik değer farkı.</summary>
        public decimal? OverrideValue { get; init; }
    }

    public sealed record ApplyPricingTemplateBulkRequestDto
    {
        public required IReadOnlyList<Guid> ProductIds { get; init; }
        public int Priority { get; init; }
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public decimal? OverrideValue { get; init; }
    }

    public sealed record ApplyPricingTemplateResultDto
    {
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public bool Succeeded { get; init; }
        public Guid? PricingRuleId { get; init; }
        public string? PricingRuleCode { get; init; }
        /// <summary>Hedef üründe birim yoksa oluşturulan ProductUnit.</summary>
        public Guid? CreatedProductUnitId { get; init; }
        /// <summary>Kurala bağlanan ürün birimi (yeni oluşturulmuş ya da mevcut).</summary>
        public Guid? LinkedProductUnitId { get; init; }
        /// <summary>Birimin bağlandığı satış planı sayısı.</summary>
        public int LinkedOfferingCount { get; init; }
        public string? Message { get; init; }
    }

    /// <summary>Var olan bir fiyatlandırma kuralından şablon üretme isteği.</summary>
    public sealed record SavePricingRuleAsTemplateRequestDto
    {
        /// <summary>Boş bırakılırsa kural adı kullanılır.</summary>
        public string? Name { get; init; }
        public string? Description { get; init; }
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (TPL-000001).</summary>
        public string? Code { get; init; }
        public bool IsActive { get; init; } = true;
    }

    /// <summary>Şablonun hangi ürünlerde kullanıldığı ve sürüm farkı.</summary>
    public sealed record PricingTemplateUsageDto
    {
        public Guid PricingRuleId { get; init; }
        public string PricingRuleCode { get; init; } = string.Empty;
        public string PricingRuleName { get; init; } = string.Empty;
        public Guid ProductId { get; init; }
        public string ProductCode { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public Guid? ProductLicenseOfferingId { get; init; }
        public string? LicenseOfferingName { get; init; }
        public int? SourceTemplateVersion { get; init; }
        public int TemplateVersion { get; init; }
        /// <summary>Kural, şablonun güncel sürümünün gerisinde kaldıysa true.</summary>
        public bool IsOutdated => SourceTemplateVersion is null || SourceTemplateVersion < TemplateVersion;
        public bool IsActive { get; init; }
    }
}
