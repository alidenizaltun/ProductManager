namespace ProductManagement.Shared.Dtos.Authentication
{
    public sealed record ResetPasswordRequestDto
    {
        public required string Email { get; init; }
        public required string Token { get; init; }
        public required string NewPassword { get; init; }
        public required string ConfirmNewPassword { get; init; }
    }
}
