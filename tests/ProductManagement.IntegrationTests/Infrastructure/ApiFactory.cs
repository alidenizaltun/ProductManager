using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// API'yi bellek içinde ayağa kaldırır.
///
/// Bu fabrika <b>veritabanına bağlanmaz</b>. Yetkilendirme sözleşmesi testleri için yeterlidir:
/// token'sız bir istek, yetkilendirme katmanında 401 alır ve hiçbir zaman DbContext'e ulaşmaz.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    /// <summary>Hiçbir sunucuya bağlanmayan, sözdizimsel olarak geçerli bağlantı dizesi.</summary>
    protected const string UnusedConnectionString =
        "Server=(local);Database=PM_Test_Unused;Trusted_Connection=True;TrustServerCertificate=True";

    protected virtual string ConnectionString => UnusedConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ActiveConnection"] = ConnectionString,
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["ConnectionStrings:HangfireConnection"] = ConnectionString,
                ["Hangfire:Enabled"] = "false",
            });
        });
    }

    /// <summary>Yönlendirmeleri takip etmeyen istemci: 302 yanıtını 200 sanmayı önler.</summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });
}
