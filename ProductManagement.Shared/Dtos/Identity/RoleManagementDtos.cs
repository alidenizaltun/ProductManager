namespace ProductManagement.Shared.Dtos.Identity
{
    public sealed record RoleDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public int UserCount { get; init; }
        public IEnumerable<string> Permissions { get; init; } = [];
        public DateTime CreatedAt { get; init; }
    }

    public sealed record CreateRoleRequestDto
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public IReadOnlyList<string> Permissions { get; init; } = [];
    }

    public sealed record UpdateRoleRequestDto
    {
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
        public IReadOnlyList<string> Permissions { get; init; } = [];
    }

    public sealed record PermissionDefinitionDto
    {
        public required string Key { get; init; }
        public required string DisplayName { get; init; }
        public required string Category { get; init; }
    }
}
