namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record InventoryTransactionDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public string? VariantSku { get; init; }
        public string? VariantName { get; init; }
        public Guid? WarehouseId { get; init; }
        public string? WarehouseName { get; init; }
        public int TransactionType { get; init; }
        public decimal Quantity { get; init; }
        public decimal? UnitCost { get; init; }
        public string? ReferenceType { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Note { get; init; }
        public DateTime OccurredAt { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public sealed record InventoryTransactionFilterDto
    {
        public Guid? ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public int? TransactionType { get; init; }
        public DateTime? DateFrom { get; init; }
        public DateTime? DateTo { get; init; }
        public int Take { get; init; } = 100;
    }

    public sealed record CreateInventoryTransactionRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public int TransactionType { get; init; }
        public decimal Quantity { get; init; }
        public decimal? UnitCost { get; init; }
        public string? ReferenceType { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Note { get; init; }
        public DateTime? OccurredAt { get; init; }
    }

    public sealed record InventoryReservationDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public string? VariantSku { get; init; }
        public string? VariantName { get; init; }
        public Guid? WarehouseId { get; init; }
        public string? WarehouseName { get; init; }
        public decimal Quantity { get; init; }
        public string ReservationCode { get; init; } = string.Empty;
        public DateTime? ReservedUntil { get; init; }
        public int Status { get; init; }
        public string? SourceType { get; init; }
        public string? SourceId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record InventoryReservationFilterDto
    {
        public Guid? ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public int? Status { get; init; }
        public DateTime? ReservedUntilMin { get; init; }
        public DateTime? ReservedUntilMax { get; init; }
        public int Take { get; init; } = 100;
    }

    public sealed record CreateInventoryReservationRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid? ProductVariantId { get; init; }
        public Guid? WarehouseId { get; init; }
        public decimal Quantity { get; init; }
        public required string ReservationCode { get; init; }
        public DateTime? ReservedUntil { get; init; }
        public int Status { get; init; } = 1;
        public string? SourceType { get; init; }
        public string? SourceId { get; init; }
    }

    public sealed record UpdateInventoryReservationStatusRequestDto
    {
        public int Status { get; init; }
        public DateTime? ReservedUntil { get; init; }
    }
}
