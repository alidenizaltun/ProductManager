using System.Text.Json;

namespace ProductManagement.Shared.Dtos.PriceEngine
{
    /// <summary>
    /// Seçilen lisans teklifine ait bir birim parametresinin değeri (ör. Kullanıcı = 25).
    /// </summary>
    public sealed record LicenseOfferingUnitInputDto
    {
        public Guid UnitDefinitionId { get; init; }
        public int Value { get; init; }
    }

    public sealed record CalculateProductPriceRequestDto
    {
        /// <summary>Yazılım ürünleri için zorunlu lisans teklifi.</summary>
        public Guid? LicenseOfferingId { get; init; }

        /// <summary>Fiziksel/dijital/paket dışı ürünler için adet. Yazılımda kullanılmaz.</summary>
        public int Quantity { get; init; } = 1;

        /// <summary>
        /// Yazılım: seçilen lisans teklifinin birim parametreleri.
        /// GET .../pricing-parameters ile dönen tüm zorunlu birimler doldurulmalıdır.
        /// </summary>
        public IReadOnlyList<LicenseOfferingUnitInputDto>? OfferingUnits { get; init; }

        public IReadOnlyList<Guid>? SelectedModuleIds { get; init; }
        public IReadOnlyList<Guid>? SelectedBundleItemIds { get; init; }
        public Guid? ProductVariantId { get; init; }
        public int? PriceType { get; init; }
        public string? PriceListCode { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public IReadOnlyDictionary<string, JsonElement>? FeatureValues { get; init; }
        public decimal? TaxRateOverride { get; init; }
        public decimal? DiscountPercent { get; init; }
        public decimal? DiscountAmount { get; init; }
        public bool PricesIncludeTax { get; init; }
    }

    /// <summary>
    /// UI'ın lisans teklifi seçildiğinde hangi birim alanlarını göstereceğini tanımlar.
    /// </summary>
    public sealed record LicenseOfferingPricingParametersDto
    {
        public Guid ProductId { get; init; }
        public Guid LicenseOfferingId { get; init; }
        public string LicenseOfferingName { get; init; } = string.Empty;
        public int LicenseModel { get; init; }
        public bool RequiresUnitInput { get; init; }
        public IReadOnlyList<LicenseOfferingUnitParameterDto> UnitParameters { get; init; } = [];
        public IReadOnlyList<PricingRuleParameterDto> RuleParameters { get; init; } = [];
    }

    public sealed record LicenseOfferingUnitParameterDto
    {
        public Guid? ProductUnitId { get; init; }
        public Guid UnitDefinitionId { get; init; }
        public string UnitDefinitionCode { get; init; } = string.Empty;
        public string UnitDefinitionName { get; init; } = string.Empty;
        public string DisplayLabel { get; init; } = string.Empty;
        public string HelpText { get; init; } = string.Empty;
        public bool IsRequired { get; init; } = true;
        public int MinValue { get; init; } = 1;
        public int? MaxValue { get; init; }
    }

    public sealed record PricingRuleParameterDto
    {
        public string Field { get; init; } = string.Empty;
        public string DisplayLabel { get; init; } = string.Empty;
        public bool IsRequired { get; init; } = true;
        public decimal MinValue { get; init; }
        public decimal? MaxValue { get; init; }
        public string? Rounding { get; init; }
    }

    public sealed record ProductPriceCalculationResultDto
    {
        public Guid ProductId { get; init; }
        public string ProductCode { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public int ProductKind { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int Quantity { get; init; }
        public Guid? LicenseOfferingId { get; init; }
        public string? LicenseOfferingName { get; init; }
        public int? LicenseModel { get; init; }
        public IReadOnlyList<AppliedLicenseOfferingUnitDto> OfferingUnits { get; init; } = [];
        public decimal SubtotalNet { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal NetBeforeTax { get; init; }
        public decimal TaxRate { get; init; }
        public string? TaxCode { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalGross { get; init; }
        public decimal? CompareAtTotalGross { get; init; }
        public bool PricesIncludeTax { get; init; }
        public IReadOnlyList<PriceCalculationLineDto> Lines { get; init; } = [];
        public DateTime CalculatedAt { get; init; }
    }

    public sealed record AppliedLicenseOfferingUnitDto
    {
        public Guid UnitDefinitionId { get; init; }
        public string? UnitDefinitionCode { get; init; }
        public string? UnitDefinitionName { get; init; }
        public string DisplayLabel { get; init; } = string.Empty;
        public int Value { get; init; }
        public decimal Amount { get; init; }
    }

    public sealed record PriceCalculationLineDto
    {
        public string LineType { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Quantity { get; init; } = 1;
        public decimal UnitAmount { get; init; }
        public decimal Amount { get; init; }
        public string? ReferenceId { get; init; }
        public string? Metadata { get; init; }
    }

    public static class PriceCalculationLineTypes
    {
        public const string LicenseBase = "LicenseBase";
        public const string Module = "Module";
        public const string UnitPrice = "UnitPrice";
        public const string PriceList = "PriceList";
        public const string PricingRule = "PricingRule";
        public const string VariantSurcharge = "VariantSurcharge";
        public const string BundleComponent = "BundleComponent";
        public const string Discount = "Discount";
        public const string Tax = "Tax";
    }
}
