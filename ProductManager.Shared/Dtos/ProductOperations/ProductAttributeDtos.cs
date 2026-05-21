namespace ProductManager.Shared.Dtos.ProductOperations
{
    public sealed record ProductAttributeDefinitionDto
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public int DataType { get; init; }
        public bool IsRequired { get; init; }
        public bool IsFilterable { get; init; }
        public bool IsVariantAxis { get; init; }
        public string? AllowedValuesJson { get; init; }
        public string? ValidationRuleJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductAttributeDefinitionRequestDto
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        public int DataType { get; init; } = 1;
        public bool IsRequired { get; init; }
        public bool IsFilterable { get; init; }
        public bool IsVariantAxis { get; init; }
        public string? AllowedValuesJson { get; init; }
        public string? ValidationRuleJson { get; init; }
    }

    public sealed record UpdateProductAttributeDefinitionRequestDto
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        public int DataType { get; init; }
        public bool IsRequired { get; init; }
        public bool IsFilterable { get; init; }
        public bool IsVariantAxis { get; init; }
        public string? AllowedValuesJson { get; init; }
        public string? ValidationRuleJson { get; init; }
    }

    public sealed record ProductAttributeValueDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid AttributeDefinitionId { get; init; }
        public string? AttributeKey { get; init; }
        public string? AttributeDisplayName { get; init; }
        public int? AttributeDataType { get; init; }
        public string? ValueText { get; init; }
        public decimal? ValueNumber { get; init; }
        public bool? ValueBool { get; init; }
        public DateTime? ValueDate { get; init; }
        public string? ValueJson { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateProductAttributeValueRequestDto
    {
        public Guid ProductId { get; init; }
        public Guid AttributeDefinitionId { get; init; }
        public string? ValueText { get; init; }
        public decimal? ValueNumber { get; init; }
        public bool? ValueBool { get; init; }
        public DateTime? ValueDate { get; init; }
        public string? ValueJson { get; init; }
    }

    public sealed record UpdateProductAttributeValueRequestDto
    {
        public string? ValueText { get; init; }
        public decimal? ValueNumber { get; init; }
        public bool? ValueBool { get; init; }
        public DateTime? ValueDate { get; init; }
        public string? ValueJson { get; init; }
    }
}
