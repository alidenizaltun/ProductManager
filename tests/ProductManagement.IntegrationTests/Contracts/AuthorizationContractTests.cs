using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Contracts;

/// <summary>
/// Yetkilendirme sözleşmesi: <b>her uç nokta ya yetki ister ya da bilinçli olarak herkese açıktır.</b>
///
/// Sabit bir uç nokta listesi tutmak yerine uygulamanın kendi yönlendirme tablosu okunur.
/// Böylece yeni eklenen korumasız bir uç nokta, kimse listeyi güncellemese bile bu testi kırar.
///
/// <para>
/// Tarihçe: 27.08.2026'da PM'nin 205 uç noktasından 149'u hiçbir yetkilendirme taşımıyordu ve bu test
/// bir <i>cırcır</i> olarak kurulmuştu — bilinen açık listelenir, büyümesi engellenirdi. 28.08.2026'da
/// izin kataloğu ürün, fiyat, katalog ve stok alanlarını kapsayacak şekilde genişletilip tüm uçlara
/// <c>[RequirePermission]</c> uygulandı. Borç kapandığı için cırcır kaldırıldı: test artık tam
/// sözleşmeyi zorluyor.
/// </para>
/// </summary>
public class AuthorizationContractTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthorizationContractTests(ApiFactory factory) => _factory = factory;

    /// <summary>
    /// Kimlik doğrulaması olmadan çağrılabilmesi kasıtlı olan yollar.
    /// Buraya bir satır eklemek bilinçli bir güvenlik kararıdır — gerekçesiz eklenmemeli.
    /// </summary>
    private static readonly HashSet<string> PubliclyReachableByDesign = new(StringComparer.OrdinalIgnoreCase)
    {
        // Oturum açma ve hesap kurtarma: kullanıcının henüz token'ı yoktur
        "POST /api/auth/login",
        "POST /api/auth/register",
        "POST /api/auth/refresh",
        "POST /api/auth/forgot-password",
        "POST /api/auth/reset-password",
        "GET /api/auth/confirm-email",

        // Herkese açık ürün vitrini: B2B paneli ve dış tüketiciler token'sız çağırır
        "GET /api/public/products",
        "GET /api/public/products/{productId}",
    };

    [Fact]
    public void Her_uc_nokta_ya_yetki_ister_ya_da_bilincli_olarak_aciktir()
    {
        var korumasiz = GetApiEndpoints()
            .Where(e => !RequiresAuthorization(e))
            .SelectMany(Describe)
            .Where(x => !PubliclyReachableByDesign.Contains(x))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            korumasiz.Count == 0,
            $"Yetkilendirmesi olmayan {korumasiz.Count} uç nokta bulundu. Her biri ya " +
            "[Authorize] / [RequirePermission] almalı ya da gerekçesiyle " +
            "PubliclyReachableByDesign listesine eklenmeli:" +
            Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", korumasiz));
    }

    [Fact]
    public async Task Korumali_bir_uc_nokta_tokensiz_istekte_401_doner()
    {
        // Sözleşmenin çalışma zamanına gerçekten yansıdığını doğrular:
        // metadata doğru olsa bile pipeline yanlış kurulmuş olabilir.
        var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Urun_uclari_da_artik_tokensiz_cagrilamaz()
    {
        // 27.08.2026'daki en ağır bulgu buydu: ürün/fiyat CRUD'u tamamen açıktı.
        var client = _factory.CreateApiClient();

        var listeleme = await client.GetAsync("/api/products");
        var fiyatListeleri = await client.GetAsync("/api/pricelists");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, listeleme.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, fiyatListeleri.StatusCode);
    }

    [Fact]
    public async Task Herkese_acik_vitrin_tokensiz_calismaya_devam_eder()
    {
        // Yetkilendirme eklenirken vitrinin kapanmadığını doğrular.
        var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/public/products");

        Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── yardımcılar ───────────────────────────────────────────────────────────

    private IEnumerable<RouteEndpoint> GetApiEndpoints()
    {
        var source = _factory.Services.GetRequiredService<EndpointDataSource>();

        return source.Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText is not null)
            .Where(e => Normalize(e.RoutePattern.RawText).StartsWith("/api/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool RequiresAuthorization(Endpoint endpoint)
    {
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null) return false;
        return endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null;
    }

    /// <summary>Rota kısıtlarını atar: <c>{id:guid}</c> → <c>{id}</c>.</summary>
    private static string Normalize(string? rawText)
    {
        var path = "/" + (rawText ?? string.Empty).TrimStart('/');
        return System.Text.RegularExpressions.Regex.Replace(path, @"\{(\w+)(:[^}]+)?\??\}", "{$1}");
    }

    private static IEnumerable<string> Describe(RouteEndpoint endpoint)
    {
        var path = Normalize(endpoint.RoutePattern.RawText);
        var verbs = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();

        if (verbs is null || verbs.HttpMethods.Count == 0)
        {
            yield return $"? {path}";
            yield break;
        }

        foreach (var verb in verbs.HttpMethods)
            yield return $"{verb} {path}";
    }
}
