namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record WarehouseDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateWarehouseRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (WH-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }
        public bool IsActive { get; init; } = true;
    }

    public sealed record UpdateWarehouseRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
