namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record ProductDto
    {
        public Guid Id { get; init; }
        public string ProductCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ShortDescription { get; init; }
        public string? Description { get; init; }
        public int Kind { get; init; }
        public int Status { get; init; }
        public string? Brand { get; init; }
        public string? Manufacturer { get; init; }
        public string? Barcode { get; init; }
        public bool IsActive { get; init; }
        public bool IsSellable { get; init; }
        public bool IsPurchasable { get; init; }
        public bool TrackInventory { get; init; }
        public string DefaultCurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public string? TaxCode { get; init; }
        public string? Tags { get; init; }
        public string? MetadataJson { get; init; }
        public string? PrimaryImageUrl { get; init; }
        public string? PrimaryThumbnailUrl { get; init; }
        public IReadOnlyList<string>? ImageUrls { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record ProductFilterDto
    {
        public string? Search { get; init; }
        public int? Kind { get; init; }
        public int? Status { get; init; }
        public bool? IsActive { get; init; }
        public int Take { get; init; } = 100;
        public bool IncludeLargeFields { get; init; } = true;
    }

    public sealed record CreateProductRequestDto
    {
        public required string ProductCode { get; init; }
        public required string Name { get; init; }
        public string? ShortDescription { get; init; }
        public string? Description { get; init; }
        public int Kind { get; init; } = 1;
        public int Status { get; init; } = 1;
        public string? Brand { get; init; }
        public string? Manufacturer { get; init; }
        public string? Barcode { get; init; }
        public bool IsActive { get; init; } = true;
        public bool IsSellable { get; init; } = true;
        public bool IsPurchasable { get; init; } = true;
        public bool TrackInventory { get; init; } = true;
        public string DefaultCurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public string? TaxCode { get; init; }
        public string? Tags { get; init; }
        public string? MetadataJson { get; init; }
    }

    public sealed record UpdateProductRequestDto
    {
        public required string ProductCode { get; init; }
        public required string Name { get; init; }
        public string? ShortDescription { get; init; }
        public string? Description { get; init; }
        public int Kind { get; init; }
        public int Status { get; init; }
        public string? Brand { get; init; }
        public string? Manufacturer { get; init; }
        public string? Barcode { get; init; }
        public bool IsActive { get; init; }
        public bool IsSellable { get; init; }
        public bool IsPurchasable { get; init; }
        public bool TrackInventory { get; init; }
        public string DefaultCurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public string? TaxCode { get; init; }
        public string? Tags { get; init; }
        public string? MetadataJson { get; init; }
    }

    public sealed record ProductDetailDto
    {
        public Guid Id { get; init; }
        public string ProductCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ShortDescription { get; init; }
        public string? Description { get; init; }
        public int Kind { get; init; }
        public int Status { get; init; }
        public string? Brand { get; init; }
        public string? Manufacturer { get; init; }
        public string? Barcode { get; init; }
        public bool IsActive { get; init; }
        public bool IsSellable { get; init; }
        public bool IsPurchasable { get; init; }
        public bool TrackInventory { get; init; }
        public string DefaultCurrencyCode { get; init; } = "TRY";
        public decimal? TaxRate { get; init; }
        public string? TaxCode { get; init; }
        public string? Tags { get; init; }
        public string? MetadataJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public IReadOnlyList<ProductAttributeValueDto> AttributeValues { get; init; } = [];
        public IReadOnlyList<ProductVariantDto> Variants { get; init; } = [];
        public IReadOnlyList<ProductPriceDto> Prices { get; init; } = [];
        public IReadOnlyList<ProductPricingRuleDto> PricingRules { get; init; } = [];
        public IReadOnlyList<ProductUnitDto> ProductUnits { get; init; } = [];
        public IReadOnlyList<ProductInventoryDto> Inventories { get; init; } = [];
        public IReadOnlyList<ProductMediaDto> MediaItems { get; init; } = [];
        public IReadOnlyList<ProductCategoryMapDto> CategoryMaps { get; init; } = [];
        public IReadOnlyList<ProductBundleItemDto> BundleItems { get; init; } = [];
        public IReadOnlyList<ProductSupplierMapDto> SupplierMaps { get; init; } = [];
        public ProductPhysicalProfileDto? PhysicalProfile { get; init; }
        public ProductSoftwareProfileDto? SoftwareProfile { get; init; }
        public ProductServiceProfileDto? ServiceProfile { get; init; }
        public ProductSubscriptionProfileDto? SubscriptionProfile { get; init; }
        public IReadOnlyList<ProductModuleDto> Modules { get; init; } = [];
        public IReadOnlyList<ProductLicenseOfferingDto> LicenseOfferings { get; init; } = [];
        public IReadOnlyList<InventoryTransactionDto> InventoryTransactions { get; init; } = [];
        public IReadOnlyList<InventoryReservationDto> InventoryReservations { get; init; } = [];
        public IReadOnlyList<ProductPriceListItemDto> PriceListItems { get; init; } = [];
    }

    public sealed record UpdateProductFullRequestDto
    {
        public required UpdateProductRequestDto Product { get; init; }
        public IReadOnlyList<CreateProductAttributeValueRequestDto>? AttributeValues { get; init; }
        public IReadOnlyList<CreateProductVariantRequestDto>? Variants { get; init; }
        public IReadOnlyList<CreateProductPriceRequestDto>? Prices { get; init; }
        public IReadOnlyList<CreateProductPricingRuleRequestDto>? PricingRules { get; init; }
        public IReadOnlyList<CreateProductUnitRequestDto>? ProductUnits { get; init; }
        public IReadOnlyList<CreateProductInventoryRequestDto>? Inventories { get; init; }
        public IReadOnlyList<CreateProductMediaRequestDto>? MediaItems { get; init; }
        public IReadOnlyList<CreateProductCategoryMapRequestDto>? CategoryMaps { get; init; }
        public IReadOnlyList<CreateProductBundleItemRequestDto>? BundleItems { get; init; }
        public IReadOnlyList<CreateProductSupplierMapRequestDto>? SupplierMaps { get; init; }
        public IReadOnlyList<CreateInventoryTransactionRequestDto>? InventoryTransactions { get; init; }
        public IReadOnlyList<CreateInventoryReservationRequestDto>? InventoryReservations { get; init; }
        public IReadOnlyList<CreateProductPriceListItemRequestDto>? PriceListItems { get; init; }
        public IReadOnlyList<CreateProductModuleRequestDto>? Modules { get; init; }
        public IReadOnlyList<CreateProductLicenseOfferingRequestDto>? LicenseOfferings { get; init; }
        public UpsertProductPhysicalProfileRequestDto? PhysicalProfile { get; init; }
        public UpsertProductSoftwareProfileRequestDto? SoftwareProfile { get; init; }
        public UpsertProductServiceProfileRequestDto? ServiceProfile { get; init; }
        public UpsertProductSubscriptionProfileRequestDto? SubscriptionProfile { get; init; }
    }

    public sealed record CreateProductFullRequestDto
    {
        public required CreateProductRequestDto Product { get; init; }
        public IReadOnlyList<CreateProductAttributeValueRequestDto>? AttributeValues { get; init; }
        public IReadOnlyList<CreateProductVariantRequestDto>? Variants { get; init; }
        public IReadOnlyList<CreateProductPriceRequestDto>? Prices { get; init; }
        public IReadOnlyList<CreateProductPricingRuleRequestDto>? PricingRules { get; init; }
        public IReadOnlyList<CreateProductUnitRequestDto>? ProductUnits { get; init; }
        public IReadOnlyList<CreateProductInventoryRequestDto>? Inventories { get; init; }
        public IReadOnlyList<CreateProductMediaRequestDto>? MediaItems { get; init; }
        public IReadOnlyList<CreateProductCategoryMapRequestDto>? CategoryMaps { get; init; }
        public IReadOnlyList<CreateProductBundleItemRequestDto>? BundleItems { get; init; }
        public IReadOnlyList<CreateProductSupplierMapRequestDto>? SupplierMaps { get; init; }
        public IReadOnlyList<CreateInventoryTransactionRequestDto>? InventoryTransactions { get; init; }
        public IReadOnlyList<CreateInventoryReservationRequestDto>? InventoryReservations { get; init; }
        public IReadOnlyList<CreateProductPriceListItemRequestDto>? PriceListItems { get; init; }
        public IReadOnlyList<CreateProductModuleRequestDto>? Modules { get; init; }
        public IReadOnlyList<CreateProductLicenseOfferingRequestDto>? LicenseOfferings { get; init; }
        public UpsertProductPhysicalProfileRequestDto? PhysicalProfile { get; init; }
        public UpsertProductSoftwareProfileRequestDto? SoftwareProfile { get; init; }
        public UpsertProductServiceProfileRequestDto? ServiceProfile { get; init; }
        public UpsertProductSubscriptionProfileRequestDto? SubscriptionProfile { get; init; }
    }
}
