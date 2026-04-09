namespace ProductManager.Shared.Dtos.Authentication
{
    public sealed record RefreshTokenRequestDto
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}
