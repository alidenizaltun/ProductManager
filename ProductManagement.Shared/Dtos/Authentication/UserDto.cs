namespace ProductManagement.Shared.Dtos.Authentication
{
    public sealed record UserDto
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
        public IEnumerable<string> Permissions { get; init; } = [];
        public DateTime CreatedAt { get; init; }
    }
}
