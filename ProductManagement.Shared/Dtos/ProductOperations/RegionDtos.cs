namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record RegionDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateRegionRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (REG-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateRegionRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record ProductRegionDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid RegionId { get; init; }
        public string? RegionCode { get; init; }
        public string? RegionName { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductRegionRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid RegionId { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductRegionRequestDto
    {
        public Guid RegionId { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }
}
