namespace ProductManagement.Shared.Dtos.Authentication
{
    public sealed record AuthResponseDto
    {
        public bool Succeeded { get; init; }
        public UserDto? User { get; init; }
        public TokenResponseDto? Token { get; init; }
        public IEnumerable<string> Errors { get; init; } = [];

        public static AuthResponseDto Success(UserDto user, TokenResponseDto token)
            => new() { Succeeded = true, User = user, Token = token };

        public static AuthResponseDto Failure(params string[] errors)
            => new() { Succeeded = false, Errors = errors };
            
        public static AuthResponseDto Failure(IEnumerable<string> errors)
            => new() { Succeeded = false, Errors = errors };
    }
}
