namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record ProductCategoryDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public Guid? ParentCategoryId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductCategoryRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (CAT-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public Guid? ParentCategoryId { get; init; }
    }

    public sealed record UpdateProductCategoryRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public Guid? ParentCategoryId { get; init; }
    }

    public sealed record ProductCategoryMapDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid ProductCategoryId { get; init; }
        public string? CategoryCode { get; init; }
        public string? CategoryName { get; init; }
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductCategoryMapRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid ProductCategoryId { get; init; }
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductCategoryMapRequestDto
    {
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
    }

    public sealed record ProductMediaDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public int MediaType { get; init; }
        public string Url { get; init; } = string.Empty;
        public string? ThumbnailUrl { get; init; }
        public string? MimeType { get; init; }
        public string? AltText { get; init; }
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductMediaRequestDto
    {
        public Guid ProductId { get; init; }
        public int MediaType { get; init; } = 1;
        public required string Url { get; init; }
        public string? ThumbnailUrl { get; init; }
        public string? MimeType { get; init; }
        public string? AltText { get; init; }
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductMediaRequestDto
    {
        public int MediaType { get; init; }
        public required string Url { get; init; }
        public string? ThumbnailUrl { get; init; }
        public string? MimeType { get; init; }
        public string? AltText { get; init; }
        public bool IsPrimary { get; init; }
        public int SortOrder { get; init; }
    }

    public sealed record ProductBundleItemDto
    {
        public Guid Id { get; init; }
        public Guid BundleProductId { get; init; }
        public Guid ChildProductId { get; init; }
        public string? ChildProductCode { get; init; }
        public string? ChildProductName { get; init; }
        public Guid? ChildVariantId { get; init; }
        public string? ChildVariantSku { get; init; }
        public string? ChildVariantName { get; init; }
        public decimal Quantity { get; init; }
        public bool IsOptional { get; init; }
        public string? RuleJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductBundleItemRequestDto
    {
        public Guid BundleProductId { get; init; }
        public Guid ChildProductId { get; init; }
        public Guid? ChildVariantId { get; init; }
        public decimal Quantity { get; init; } = 1;
        public bool IsOptional { get; init; }
        public string? RuleJson { get; init; }
    }

    public sealed record UpdateProductBundleItemRequestDto
    {
        public Guid ChildProductId { get; init; }
        public Guid? ChildVariantId { get; init; }
        public decimal Quantity { get; init; }
        public bool IsOptional { get; init; }
        public string? RuleJson { get; init; }
    }
}
