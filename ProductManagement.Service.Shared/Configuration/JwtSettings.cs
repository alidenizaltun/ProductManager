namespace ProductManagement.Service.Shared.Configuration
{
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";
        public required string Key { get; init; }
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public int AccessTokenExpirationMinutes { get; init; } = 60;
        public int RefreshTokenExpirationDays { get; init; } = 7;
        public int RememberMeAccessTokenExpirationMinutes { get; init; } = 1440;
        public int RememberMeRefreshTokenExpirationDays { get; init; } = 30;
        public int ClockSkewMinutes { get; init; } = 5;
    }
}
