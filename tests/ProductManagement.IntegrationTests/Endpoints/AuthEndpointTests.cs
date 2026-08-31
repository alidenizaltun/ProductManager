using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Kimlik doğrulama uç noktalarının davranış sözleşmesi.
///
/// Bu uçlar <c>[AllowAnonymous]</c>/<c>[Authorize]</c> ile çalışır, <c>[RequirePermission]</c>
/// ile değil — bu yüzden veri testlerindeki <c>TestAuthHandler</c>'ın sabit "Admin" kimliği
/// devreye girmez ya da isteğe bağlı olarak devreye girer ama karşılık gelen bir DB kaydı
/// yoktur (<c>TestAuthHandler.UserId</c> gerçek bir kullanıcıya işaret etmez — bkz.
/// <c>Beni_getir_...</c> testi). Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class AuthEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public AuthEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static object YeniKayit(string? email = null) => new
    {
        Email = email ?? $"auth_test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid",
        Password = "Test123!",
        ConfirmPassword = "Test123!",
        FirstName = "Test",
        LastName = "Kullanıcı",
    };

    [DockerFact]
    public async Task Kayit_yeni_kullaniciyla_basarili_olur()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", YeniKayit());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponsePayload>();
        Assert.True(body!.Succeeded);
    }

    [DockerFact]
    public async Task Ayni_epostayla_ikinci_kayit_400_doner()
    {
        var kayit = YeniKayit();
        var ilk = await Client.PostAsJsonAsync("/api/auth/register", kayit);
        ilk.EnsureSuccessStatusCode();

        var ikinci = await Client.PostAsJsonAsync("/api/auth/register", kayit);

        Assert.Equal(HttpStatusCode.BadRequest, ikinci.StatusCode);
    }

    [DockerFact]
    public async Task Sifreler_eslesmezse_400_doner()
    {
        var email = $"auth_test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid";
        var gecersiz = new { Email = email, Password = "Test123!", ConfirmPassword = "Farkli123!", FirstName = "Ad", LastName = "Soyad" };

        var response = await Client.PostAsJsonAsync("/api/auth/register", gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Giris_gecerli_bilgilerle_basarili_olur()
    {
        var email = $"auth_test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid";
        var kayit = await Client.PostAsJsonAsync("/api/auth/register", YeniKayit(email));
        kayit.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "Test123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponsePayload>();
        Assert.True(body!.Succeeded);
    }

    [DockerFact]
    public async Task Giris_yanlis_sifreyle_401_doner()
    {
        var email = $"auth_test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid";
        var kayit = await Client.PostAsJsonAsync("/api/auth/register", YeniKayit(email));
        kayit.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "YanlisSifre1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerFact]
    public async Task Giris_olmayan_kullaniciyla_401_doner()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new { Email = "hic-olmayan@example.invalid", Password = "Test123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerFact]
    public async Task Sifremi_unuttum_bilinmeyen_epostada_bile_200_doner()
    {
        // Kullanıcı numaralandırmasını önlemek için: var/yok fark etmeksizin her zaman 200.
        var response = await Client.PostAsJsonAsync(
            "/api/auth/forgot-password", new { Email = "hic-olmayan@example.invalid" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Gecersiz_token_ile_eposta_onayi_400_doner()
    {
        var response = await Client.GetAsync($"/api/auth/confirm-email?userId={Guid.NewGuid()}&token=gecersiz-token");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Gecersiz_token_ile_sifre_sifirlama_400_doner()
    {
        var email = $"auth_test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid";
        var kayit = await Client.PostAsJsonAsync("/api/auth/register", YeniKayit(email));
        kayit.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            Email = email,
            Token = "gecersiz-token",
            NewPassword = "YeniSifre1!",
            ConfirmNewPassword = "YeniSifre1!",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Beni_getir_var_olmayan_test_kullanicisi_icin_404_doner()
    {
        // DatabaseApiFactory'deki TestAuthHandler her isteği sabit bir Admin kimliğiyle
        // doğrular ama bu kimliğe karşılık gelen gerçek bir ApplicationUser satırı yoktur.
        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Cikis_yap_var_olmayan_kullanici_icin_bile_204_doner()
    {
        var response = await Client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private sealed record AuthResponsePayload(bool Succeeded, IEnumerable<string> Errors);
}
