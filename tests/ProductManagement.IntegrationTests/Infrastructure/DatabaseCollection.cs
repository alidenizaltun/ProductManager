namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// SQL Server konteynerini tüm veri testleri arasında <b>bir kez</b> ayağa kaldırır.
/// Konteyner başlatma ve 19 migration'ın uygulanması pahalı olduğu için, veri testi
/// yazan her sınıf bu koleksiyona katılmalıdır.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}

/// <summary>Konteyner ömrünü yöneten paylaşımlı fikstür.</summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private DatabaseApiFactory? _factory;

    /// <summary>
    /// Fabrika yalnızca Docker varken kurulur.
    ///
    /// Testcontainers, konteyner nesnesini <i>inşa ederken</i> Docker uç noktasını çözmeye
    /// çalışır; Docker kapalıyken bu çağrı uzun süre bloke olur. Bu yüzden nesne alan
    /// başlatıcısında değil, burada geç oluşturulur.
    /// </summary>
    public DatabaseApiFactory Factory =>
        _factory ?? throw new InvalidOperationException(
            "Docker çalışmıyor; veri testleri DockerFact ile atlanmalıydı.");

    public Task InitializeAsync()
    {
        if (!DockerEnvironment.IsAvailable) return Task.CompletedTask;

        _factory = new DatabaseApiFactory();
        return _factory.InitializeAsync();
    }

    public Task DisposeAsync()
        => _factory?.DisposeAsync() ?? Task.CompletedTask;
}
