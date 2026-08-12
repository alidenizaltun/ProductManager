namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record ProductSupplierDto
    {
        public Guid Id { get; init; }
        public string SupplierCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? TaxNumber { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Address { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductSupplierRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (SUP-000001).</summary>
        public string? SupplierCode { get; init; }
        public required string Name { get; init; }
        public string? TaxNumber { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Address { get; init; }
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateProductSupplierRequestDto
    {
        public required string SupplierCode { get; init; }
        public required string Name { get; init; }
        public string? TaxNumber { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Address { get; init; }
        public bool IsActive { get; init; } = true;
    }

    public sealed record ProductSupplierMapDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid ProductSupplierId { get; init; }
        public string? SupplierCode { get; init; }
        public string? SupplierName { get; init; }
        public string? SupplierProductCode { get; init; }
        public decimal? SupplierCost { get; init; }
        public int? LeadTimeInDays { get; init; }
        public decimal? MinOrderQuantity { get; init; }
        public bool IsPreferred { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductSupplierMapRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid ProductSupplierId { get; init; }
        public string? SupplierProductCode { get; init; }
        public decimal? SupplierCost { get; init; }
        public int? LeadTimeInDays { get; init; }
        public decimal? MinOrderQuantity { get; init; }
        public bool IsPreferred { get; init; }
    }

    public sealed record UpdateProductSupplierMapRequestDto
    {
        public string? SupplierProductCode { get; init; }
        public decimal? SupplierCost { get; init; }
        public int? LeadTimeInDays { get; init; }
        public decimal? MinOrderQuantity { get; init; }
        public bool IsPreferred { get; init; }
    }
}
