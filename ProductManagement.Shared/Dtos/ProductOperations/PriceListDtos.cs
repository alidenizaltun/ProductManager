namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record ProductPriceListDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductPriceListRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (PL-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
    }

    public sealed record UpdateProductPriceListRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public bool IsActive { get; init; } = true;
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
    }

    public sealed record ProductPriceListItemDto
    {
        public Guid Id { get; init; }
        public Guid ProductPriceListId { get; init; }
        public string? PriceListCode { get; init; }
        public string? PriceListName { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public string? VariantSku { get; init; }
        public string? VariantName { get; init; }
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductPriceListItemRequestDto
    {
        public Guid ProductPriceListId { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
    }

    public sealed record UpdateProductPriceListItemRequestDto
    {
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
    }
}
