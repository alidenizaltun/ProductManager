using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.EfCore.Context;
using Testcontainers.MsSql;

namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// Gerçek bir SQL Server'a karşı çalışan API fabrikası.
///
/// Her koşuda Docker üzerinde tek kullanımlık bir SQL Server konteyneri açılır ve
/// EF Core migration'ları uygulanır. Paylaşımlı geliştirme veritabanına <b>asla</b>
/// dokunulmaz — testler veri yazıp sildiği için bu bilinçli bir kuraldır.
/// </summary>
public sealed class DatabaseApiFactory : ApiFactory, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test!Passw0rd.2026")
        .Build();

    private string? _connectionString;

    protected override string ConnectionString =>
        _connectionString ?? throw new InvalidOperationException(
            "Konteyner henüz başlatılmadı. Bu fabrikayı IAsyncLifetime üzerinden kullanın.");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // Veri testleri yetkilendirmeyi değil davranışı doğrular; kimlik doğrulama
            // ayrı bir test sınıfının konusudur (bkz. AuthorizationContractTests).
            services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

            // Uygulama üç şema alanını da JwtBearer'a sabitliyor. Yalnızca DefaultScheme'i
            // değiştirmek yetmez: DefaultAuthenticateScheme ve DefaultChallengeScheme
            // daha özel oldukları için kazanır ve istek 401 alır.
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();
        _connectionString = _sqlServer.GetConnectionString();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        GuardAgainstNonContainerDatabase(db.Database.GetConnectionString());

        await db.Database.MigrateAsync();
    }

    /// <summary>
    /// Migration çalıştırmadan önce hedefin gerçekten test konteyneri olduğunu doğrular.
    ///
    /// Bu koruma bir kez gerçekten gerekti: yapılandırma anahtarı yanlış yazıldığı için
    /// override sessizce etkisiz kaldı ve testler <c>appsettings.json</c>'daki paylaşımlı
    /// dev veritabanına yazdı. Bir yazım hatası asla sessizce üretime yakın veriye
    /// dokunmamalı — bu yüzden yüksek sesle patlıyoruz.
    /// </summary>
    private void GuardAgainstNonContainerDatabase(string? actual)
    {
        var beklenen = ConnectionString;

        // Ham dize karşılaştırılamaz: SqlClient dizeyi normalize eder
        // (Server → Data Source, Database → Initial Catalog, Application Name ekler).
        // Güvenliği belirleyen tek alan sunucu adresidir — onu karşılaştırırız.
        if (SunucuAdresi(actual) == SunucuAdresi(beklenen)) return;

        throw new InvalidOperationException(
            "Test veritabanı bağlantısı test konteynerine gitmiyor — migration durduruldu." +
            Environment.NewLine +
            $"  Beklenen : {Maskele(beklenen)}" + Environment.NewLine +
            $"  Gerçekleşen: {Maskele(actual)}" + Environment.NewLine +
            "Muhtemel sebep: uygulamanın okuduğu yapılandırma anahtarı ile ApiFactory'de " +
            "yazılan anahtar farklı. Bkz. ConfigurationExtensions.GetActiveConnectionString.");
    }

    /// <summary>Bağlantı dizesinden sunucu adresini çıkarır; ayrıştırılamazsa boş döner.</summary>
    private static string SunucuAdresi(string? cs)
    {
        if (string.IsNullOrWhiteSpace(cs)) return string.Empty;

        try
        {
            return new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs)
                .DataSource.Trim().ToLowerInvariant();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string Maskele(string? cs) =>
        cs is null ? "(yok)" : System.Text.RegularExpressions.Regex.Replace(cs, "Password=[^;]*", "Password=***");

    public new async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
        await base.DisposeAsync();
    }
}
