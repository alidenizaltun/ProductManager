namespace ProductManager.Shared.Dtos.Authentication
{
    public sealed record LoginRequestDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public bool RememberMe { get; init; } = false;
    }
}
