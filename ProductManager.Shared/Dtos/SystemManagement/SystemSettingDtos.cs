namespace ProductManager.Shared.Dtos.SystemManagement
{
    public sealed record SystemSettingDto
    {
        public Guid Id { get; init; }
        public required string Category { get; init; }
        public required string Key { get; init; }
        public string? Value { get; init; }
        public required string DataType { get; init; }
        public required string DisplayName { get; init; }
        public string? Description { get; init; }
        public bool IsEditable { get; init; }
        public int SortOrder { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record UpdateSystemSettingItemDto
    {
        public Guid Id { get; init; }
        public string? Value { get; init; }
    }

    public sealed record BulkUpdateSystemSettingsRequestDto
    {
        public IReadOnlyList<UpdateSystemSettingItemDto> Items { get; init; } = [];
    }
}
