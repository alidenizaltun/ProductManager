using System.Text.Json.Serialization;

namespace ProductManagement.Shared.Dtos.ProductOperations
{
    // ─── ProductModule ───────────────────────────────────────────────

    public sealed record ProductModuleDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string ModuleCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsOptional { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public IReadOnlyList<ProductModuleOfferingPriceDto> OfferingPrices { get; init; } = [];
    }

    public sealed record CreateProductModuleRequestDto
    {
        public Guid ProductId { get; init; }
        public required string ModuleCode { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsOptional { get; init; } = true;
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
        // Full create akışında kullanılır; modüle ait lisans teklifi fiyatları
        public IReadOnlyList<CreateModuleOfferingPriceInlineDto>? OfferingPrices { get; init; }
    }

    public sealed record CreateModuleOfferingPriceInlineDto
    {
        public string? LicenseOfferingTempId { get; init; }
        public Guid? ProductLicenseOfferingId { get; init; }
        public decimal Price { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateProductModuleRequestDto
    {
        public required string ModuleCode { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsOptional { get; init; } = true;
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    // ─── ProductModuleOfferingPrice ──────────────────────────────────

    public sealed record ProductModuleOfferingPriceDto
    {
        public Guid Id { get; init; }
        public Guid ProductModuleId { get; init; }
        public string? ModuleCode { get; init; }
        public string? ModuleName { get; init; }
        public Guid ProductLicenseOfferingId { get; init; }
        public string? LicenseOfferingName { get; init; }
        public decimal Price { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductModuleOfferingPriceRequestDto
    {
        public Guid ProductModuleId { get; init; }
        public Guid ProductLicenseOfferingId { get; init; }
        public decimal Price { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateProductModuleOfferingPriceRequestDto
    {
        public decimal Price { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
    }

    // ─── ProductLicenseOffering ──────────────────────────────────────

    public sealed record ProductLicenseOfferingDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public IReadOnlyList<Guid> ProductUnitIds { get; init; } = [];
        public IReadOnlyList<ProductUnitDto> ProductUnits { get; init; } = [];
        public int LicenseModel { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal BasePrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? BillingPeriodUnit { get; init; }
        public int? BillingPeriodValue { get; init; }
        public bool AutoRenew { get; init; }
        public int? GracePeriodDays { get; init; }
        public int? TrialDays { get; init; }
        public Guid? ConvertToOfferingId { get; init; }
        public string? ConvertToOfferingName { get; init; }
        public int? MaxSeats { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductLicenseOfferingRequestDto
    {
        public Guid? Id { get; init; }
        [JsonPropertyName("_tempId")]
        public string? TempId { get; init; }
        public IReadOnlyList<string>? ProductUnitTempIds { get; init; }
        public Guid ProductId { get; init; }
        public IReadOnlyList<Guid>? ProductUnitIds { get; init; }
        public int LicenseModel { get; init; } = 1;
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal BasePrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? BillingPeriodUnit { get; init; }
        public int? BillingPeriodValue { get; init; }
        public bool AutoRenew { get; init; } = true;
        public int? GracePeriodDays { get; init; }
        public int? TrialDays { get; init; }
        public Guid? ConvertToOfferingId { get; init; }
        public int? MaxSeats { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductLicenseOfferingRequestDto
    {
        public int LicenseModel { get; init; }
        public IReadOnlyList<Guid>? ProductUnitIds { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal BasePrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? BillingPeriodUnit { get; init; }
        public int? BillingPeriodValue { get; init; }
        public bool AutoRenew { get; init; } = true;
        public int? GracePeriodDays { get; init; }
        public int? TrialDays { get; init; }
        public Guid? ConvertToOfferingId { get; init; }
        public int? MaxSeats { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }
}
