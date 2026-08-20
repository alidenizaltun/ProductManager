namespace ProductManagement.Shared.Dtos.Identity
{
    public sealed record AdminUserDto
    {
        public Guid Id { get; init; }
        public required string Email { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? PhoneNumber { get; init; }
        public bool EmailConfirmed { get; init; }
        public bool IsActive { get; init; }
        public IEnumerable<string> Roles { get; init; } = [];
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateUserRequestDto
    {
        public required string Email { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = [];
    }

    public sealed record UpdateUserRequestDto
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public bool IsActive { get; init; } = true;
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
