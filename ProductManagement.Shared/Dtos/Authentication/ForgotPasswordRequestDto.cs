namespace ProductManagement.Shared.Dtos.Authentication
{
    public sealed record ForgotPasswordRequestDto
    {
        public required string Email { get; init; }
    }
}
