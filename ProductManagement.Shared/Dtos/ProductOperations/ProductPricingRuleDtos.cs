using System.Text.Json;

namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record ProductPricingRuleDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string PriceAdjustmentJson { get; init; } = string.Empty;
        public int Priority { get; init; }
        public bool IsActive { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? ProductLicenseOfferingId { get; init; }
        public IReadOnlyList<Guid> ProductUnitIds { get; init; } = [];
        public IReadOnlyList<ProductUnitDto> ProductUnits { get; init; } = [];
        /// <summary>Kural bir fiyat şablonundan kopyalandıysa kaynağı.</summary>
        public Guid? SourceTemplateId { get; init; }
        public string? SourceTemplateCode { get; init; }
        public string? SourceTemplateName { get; init; }
        public int? SourceTemplateVersion { get; init; }
        /// <summary>Şablonun güncel sürümü. SourceTemplateVersion'dan büyükse kural geride kalmıştır.</summary>
        public int? TemplateCurrentVersion { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductPricingRuleRequestDto
    {
        public Guid ProductId { get; init; }
        public string? LicenseOfferingTempId { get; init; }
        public IReadOnlyList<string>? ProductUnitTempIds { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? PriceAdjustmentJson { get; init; }
        public JsonElement? PriceAdjustment { get; init; }
        public int Priority { get; init; }
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? ProductLicenseOfferingId { get; init; }
        public IReadOnlyList<Guid>? ProductUnitIds { get; init; }
        /// <summary>Kural bir şablondan üretiliyorsa kaynağın izi. İstemci doldurmaz, şablon uygulama akışı doldurur.</summary>
        public Guid? SourceTemplateId { get; init; }
        public int? SourceTemplateVersion { get; init; }
    }

    public sealed record ReorderProductPricingRulesRequestDto
    {
        public required IReadOnlyList<Guid> OrderedPricingRuleIds { get; init; }
    }

    public sealed record UpdateProductPricingRuleRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? PriceAdjustmentJson { get; init; }
        public JsonElement? PriceAdjustment { get; init; }
        public int Priority { get; init; }
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? ProductLicenseOfferingId { get; init; }
        public IReadOnlyList<Guid>? ProductUnitIds { get; init; }
    }
}
