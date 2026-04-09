namespace ProductManager.Shared.Dtos.Authentication
{
    public sealed record RegisterRequestDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string ConfirmPassword { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? PhoneNumber { get; init; }
    }
}
