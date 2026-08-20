namespace ProductManagement.Shared.Dtos.SystemManagement
{
    public sealed record IntegrationDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required string ProviderKey { get; init; }
        public bool IsEnabled { get; init; }
        public string? ConfigJson { get; init; }
        public bool HasCredentials { get; init; }
        public string? CredentialsPreview { get; init; }
        public bool IsSystemManaged { get; init; }
        public string? Description { get; init; }
        public DateTime? LastTestedAt { get; init; }
        public bool? LastTestSucceeded { get; init; }
        public string? LastTestMessage { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public sealed record CreateIntegrationRequestDto
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required string ProviderKey { get; init; }
        public bool IsEnabled { get; init; } = true;
        public string? ConfigJson { get; init; }
        public Dictionary<string, string>? Credentials { get; init; }
        public string? Description { get; init; }
    }

    public sealed record UpdateIntegrationRequestDto
    {
        public required string Name { get; init; }
        public bool IsEnabled { get; init; } = true;
        public string? ConfigJson { get; init; }
        public Dictionary<string, string>? Credentials { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>Repository&lt;-&gt;servis katmanı arası ham veri taşıyıcı; CredentialsProtected asla API'ye dönmez.</summary>
    public sealed record IntegrationRecordDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required string ProviderKey { get; init; }
        public bool IsEnabled { get; init; }
        public string? ConfigJson { get; init; }
        public string? CredentialsProtected { get; init; }
        public bool IsSystemManaged { get; init; }
        public string? Description { get; init; }
        public DateTime? LastTestedAt { get; init; }
        public bool? LastTestSucceeded { get; init; }
        public string? LastTestMessage { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
