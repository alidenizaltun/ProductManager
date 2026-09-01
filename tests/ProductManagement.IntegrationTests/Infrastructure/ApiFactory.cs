using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Service.Shared.Abstract;

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

        // Uygulama Kestrel'i açıkça yapılandırıyor ve sabit bir porta bağlanabiliyor.
        // Port 0 işletim sisteminden boş port ister; böylece aynı anda koşan iki test
        // süreci (ör. iki ayrı oturum) "address already in use" ile çakışmaz.
        builder.UseSetting("urls", "http://127.0.0.1:0");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // DİKKAT: Uygulama bağlantıyı `ConnectionStrings:Default` anahtarından okur
            // (bkz. ConfigurationExtensions.GetActiveConnectionString). Başka bir ad
            // kullanmak override'ı sessizce etkisiz bırakır ve testler appsettings.json'daki
            // PAYLAŞIMLI DEV VERİTABANINA gider. Bu anahtar adı değiştirilmemeli.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["ConnectionStrings:Hangfire"] = ConnectionString,
                ["Hangfire:Enabled"] = "false",
            });
        });

        // Gerçek DevaGateway kimlik bilgileri Testing ortamı için geçersiz kılınmamış;
        // e-posta gönderen her akış gerçek sağlayıcı kotasını tüketmesin diye sahtesiyle değiştirilir.
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<IEmailService, NoOpEmailService>();
        });
    }

    /// <summary>Yönlendirmeleri takip etmeyen istemci: 302 yanıtını 200 sanmayı önler.</summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });
}
