namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record ProductVariantDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string Sku { get; init; } = string.Empty;
        public string? Barcode { get; init; }
        public string? Name { get; init; }
        public string? OptionValuesJson { get; init; }
        public decimal? AdditionalPrice { get; init; }
        public decimal? AdditionalCost { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductVariantRequestDto
    {
        public Guid ProductId { get; init; }
        public required string Sku { get; init; }
        public string? Barcode { get; init; }
        public string? Name { get; init; }
        public string? OptionValuesJson { get; init; }
        public decimal? AdditionalPrice { get; init; }
        public decimal? AdditionalCost { get; init; }
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateProductVariantRequestDto
    {
        public required string Sku { get; init; }
        public string? Barcode { get; init; }
        public string? Name { get; init; }
        public string? OptionValuesJson { get; init; }
        public decimal? AdditionalPrice { get; init; }
        public decimal? AdditionalCost { get; init; }
        public bool IsActive { get; init; }
    }

    public sealed record ProductPriceDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public string? VariantSku { get; init; }
        public string? VariantName { get; init; }
        public int PriceType { get; init; }
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductPriceRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public int PriceType { get; init; } = 1;
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
    }

    public sealed record UpdateProductPriceRequestDto
    {
        public Guid? ProductVariantId { get; init; }
        public int PriceType { get; init; }
        public decimal Amount { get; init; }
        public decimal? CompareAtAmount { get; init; }
        public string CurrencyCode { get; init; } = "TRY";
        public int? MinQuantity { get; init; }
        public int? MaxQuantity { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
        public string? SalesChannel { get; init; }
        public string? CustomerGroupCode { get; init; }
    }

    public sealed record ProductInventoryDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public string? VariantSku { get; init; }
        public string? VariantName { get; init; }
        public Guid? WarehouseId { get; init; }
        public string WarehouseCode { get; init; } = string.Empty;
        public string? WarehouseName { get; init; }
        public decimal QuantityOnHand { get; init; }
        public decimal QuantityReserved { get; init; }
        public decimal QuantityAvailable { get; init; }
        public decimal? ReorderPoint { get; init; }
        public decimal? ReorderQuantity { get; init; }
        public int InventoryPolicy { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record ProductInventoryFilterDto
    {
        public Guid? ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public int Take { get; init; } = 100;
    }

    public sealed record CreateProductInventoryRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public required string WarehouseCode { get; init; }
        public decimal QuantityOnHand { get; init; }
        public decimal QuantityReserved { get; init; }
        public decimal? ReorderPoint { get; init; }
        public decimal? ReorderQuantity { get; init; }
        public int InventoryPolicy { get; init; } = 1;
    }

    public sealed record UpdateProductInventoryRequestDto
    {
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public required string WarehouseCode { get; init; }
        public decimal QuantityOnHand { get; init; }
        public decimal QuantityReserved { get; init; }
        public decimal? ReorderPoint { get; init; }
        public decimal? ReorderQuantity { get; init; }
        public int InventoryPolicy { get; init; }
    }
}
