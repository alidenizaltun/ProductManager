namespace ProductManagement.Shared.Dtos.Authentication
{
    public sealed record TokenResponseDto
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public int ExpiresIn { get; init; }
        public string TokenType { get; init; } = "Bearer";
        public DateTime ExpiresAt { get; init; }
    }
}
