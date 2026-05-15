namespace ProductManager.Shared.Dtos.ProductOperations
{
    // ─── ProductModule ───────────────────────────────────────────────

    public sealed record ProductModuleDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string ModuleCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal AdditionalPrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsOptional { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductModuleRequestDto
    {
        public required Guid ProductId { get; init; }
        public required string ModuleCode { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal AdditionalPrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsOptional { get; init; } = true;
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductModuleRequestDto
    {
        public required string ModuleCode { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public decimal AdditionalPrice { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsOptional { get; init; } = true;
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    // ─── SoftwarePricingTier ─────────────────────────────────────────

    public sealed record SoftwarePricingTierDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid ProductLicenseOfferingId { get; init; }
        public string? LicenseOfferingName { get; init; }
        public Guid UnitDefinitionId { get; init; }
        public string? UnitDefinitionName { get; init; }
        public int MinUnits { get; init; }
        public int? MaxUnits { get; init; }
        public decimal PricePerUnit { get; init; }
        public decimal FlatFee { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateSoftwarePricingTierRequestDto
    {
        public required Guid ProductId { get; init; }
        public required Guid ProductLicenseOfferingId { get; init; }
        public required Guid UnitDefinitionId { get; init; }
        public int MinUnits { get; init; }
        public int? MaxUnits { get; init; }
        public decimal PricePerUnit { get; init; }
        public decimal FlatFee { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateSoftwarePricingTierRequestDto
    {
        public required Guid ProductLicenseOfferingId { get; init; }
        public required Guid UnitDefinitionId { get; init; }
        public int MinUnits { get; init; }
        public int? MaxUnits { get; init; }
        public decimal PricePerUnit { get; init; }
        public decimal FlatFee { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
    }

    // ─── ProductLicenseOffering ──────────────────────────────────────

    public sealed record ProductLicenseOfferingDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
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
        public required Guid ProductId { get; init; }
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
