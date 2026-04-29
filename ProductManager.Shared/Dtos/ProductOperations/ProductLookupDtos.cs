namespace ProductManager.Shared.Dtos.ProductOperations
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
    }
}
