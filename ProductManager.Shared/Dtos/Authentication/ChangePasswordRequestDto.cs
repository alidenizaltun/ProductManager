namespace ProductManager.Shared.Dtos.Authentication
{
    public sealed record ChangePasswordRequestDto
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
        public required string ConfirmNewPassword { get; init; }
    }
}
