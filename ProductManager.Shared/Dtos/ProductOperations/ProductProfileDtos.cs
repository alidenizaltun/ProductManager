namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record ProductPhysicalProfileDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public decimal? Weight { get; init; }
        public decimal? Width { get; init; }
        public decimal? Height { get; init; }
        public decimal? Length { get; init; }
        public bool RequiresShipping { get; init; }
        public bool IsFragile { get; init; }
        public bool IsHazardous { get; init; }
        public bool RequiresSerialNumber { get; init; }
        public int? WarrantyInMonths { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record UpsertProductPhysicalProfileRequestDto
    {
        public decimal? Weight { get; init; }
        public decimal? Width { get; init; }
        public decimal? Height { get; init; }
        public decimal? Length { get; init; }
        public bool RequiresShipping { get; init; } = true;
        public bool IsFragile { get; init; }
        public bool IsHazardous { get; init; }
        public bool RequiresSerialNumber { get; init; }
        public int? WarrantyInMonths { get; init; }
    }

    public sealed record ProductSoftwareProfileDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string? Version { get; init; }
        public int LicenseModel { get; init; }
        public int? SeatCount { get; init; }
        public string? DownloadUrl { get; init; }
        public string? SupportedPlatformsJson { get; init; }
        public string? SystemRequirementsJson { get; init; }
        public string? ReleaseNotes { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record UpsertProductSoftwareProfileRequestDto
    {
        public string? Version { get; init; }
        public int LicenseModel { get; init; } = 1;
        public int? SeatCount { get; init; }
        public string? DownloadUrl { get; init; }
        public string? SupportedPlatformsJson { get; init; }
        public string? SystemRequirementsJson { get; init; }
        public string? ReleaseNotes { get; init; }
    }

    public sealed record ProductServiceProfileDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public int DeliveryMode { get; init; }
        public int? DurationInMinutes { get; init; }
        public int? MaxConcurrentBooking { get; init; }
        public string? ServiceAreaJson { get; init; }
        public string? ServiceLevelAgreementJson { get; init; }
        public string? CapacityRuleJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record UpsertProductServiceProfileRequestDto
    {
        public int DeliveryMode { get; init; } = 2;
        public int? DurationInMinutes { get; init; }
        public int? MaxConcurrentBooking { get; init; }
        public string? ServiceAreaJson { get; init; }
        public string? ServiceLevelAgreementJson { get; init; }
        public string? CapacityRuleJson { get; init; }
    }

    public sealed record ProductSubscriptionProfileDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public int BillingPeriodUnit { get; init; }
        public int BillingPeriodValue { get; init; }
        public int? TrialDays { get; init; }
        public bool AutoRenew { get; init; }
        public int? GracePeriodDays { get; init; }
        public string? CancellationPolicy { get; init; }
        public string? SubscriptionRulesJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record UpsertProductSubscriptionProfileRequestDto
    {
        public int BillingPeriodUnit { get; init; } = 3;
        public int BillingPeriodValue { get; init; } = 1;
        public int? TrialDays { get; init; }
        public bool AutoRenew { get; init; } = true;
        public int? GracePeriodDays { get; init; }
        public string? CancellationPolicy { get; init; }
        public string? SubscriptionRulesJson { get; init; }
    }
}
