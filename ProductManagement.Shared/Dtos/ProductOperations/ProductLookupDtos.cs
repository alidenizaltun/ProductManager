namespace ProductManagement.Shared.Dtos.ProductOperations
{
    public sealed record LookupItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public sealed record ProductReferenceLookupsDto
    {
        public IReadOnlyList<LookupItemDto> Products { get; init; } = [];
        public IReadOnlyList<LookupItemDto> Categories { get; init; } = [];
        public IReadOnlyList<LookupItemDto> Warehouses { get; init; } = [];
        public IReadOnlyList<LookupItemDto> Suppliers { get; init; } = [];
        public IReadOnlyList<LookupItemDto> PriceLists { get; init; } = [];
        public IReadOnlyList<LookupItemDto> UnitDefinitions { get; init; } = [];
    }

    public sealed record UnitDefinitionDto
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

    public sealed record CreateUnitDefinitionRequestDto
    {
        /// <summary>Boş bırakılırsa kod sistem tarafından üretilir (UNIT-000001).</summary>
        public string? Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateUnitDefinitionRequestDto
    {
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record ProductUnitDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid UnitDefinitionId { get; init; }
        public string? UnitDefinitionCode { get; init; }
        public string? UnitDefinitionName { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int Role { get; init; } = 1;
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductUnitRequestDto
    {
        public Guid? Id { get; init; }
        [System.Text.Json.Serialization.JsonPropertyName("_tempId")]
        public string? TempId { get; init; }
        public Guid ProductId { get; init; }
        public Guid UnitDefinitionId { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int Role { get; init; } = 1;
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }

    public sealed record UpdateProductUnitRequestDto
    {
        public Guid UnitDefinitionId { get; init; }
        public required string Code { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int Role { get; init; } = 1;
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; } = true;
        public int SortOrder { get; init; }
    }
}
