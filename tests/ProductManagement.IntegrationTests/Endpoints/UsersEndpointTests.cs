using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Kullanıcı yönetimi uç noktalarının davranış sözleşmesi.
///
/// Oluşturma ve davetiyeyi yeniden gönderme, davet e-postası göndermeye çalışır — ama
/// <c>EmailService.SendEmailAsync</c> tüm istisnaları yutup loglar, hiçbir zaman fırlatmaz.
/// Bu yüzden testler gerçek bir e-posta sağlayıcısına bağlı olmadan güvenle koşar.
/// Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class UsersEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public UsersEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static object YeniKullanici() => new
    {
        Email = $"test_{Guid.NewGuid().ToString("N")[..8]}@example.invalid",
        FirstName = "Test",
        LastName = "Kullanıcı",
        Roles = Array.Empty<string>(),
    };

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync("/api/users", YeniKullanici());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<UserPayload>();
        Assert.False(created!.EmailConfirmed);
    }

    [DockerFact]
    public async Task Gecersiz_eposta_400_doner()
    {
        var gecersiz = new { Email = "gecersiz-eposta", FirstName = "Test", Roles = Array.Empty<string>() };

        var response = await Client.PostAsJsonAsync("/api/users", gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Ayni_epostayla_ikinci_kez_olusturmak_409_doner()
    {
        var payload = YeniKullanici();
        var ilk = await Client.PostAsJsonAsync("/api/users", payload);
        ilk.EnsureSuccessStatusCode();

        var ikinci = await Client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.Conflict, ikinci.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await KullaniciOlusturVeOku();

        var response = await Client.GetAsync($"/api/users/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await KullaniciOlusturVeOku();
        var guncel = new { FirstName = "Güncel", LastName = "Ad", IsActive = true, Roles = Array.Empty<string>() };

        var response = await Client.PutAsJsonAsync($"/api/users/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<UserPayload>($"/api/users/{created.Id}");
        Assert.Equal("Güncel", okunan!.FirstName);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new { FirstName = "Ad", IsActive = true, Roles = Array.Empty<string>() };

        var response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Pasiflestirilen_kullanici_artik_aktif_degildir()
    {
        var created = await KullaniciOlusturVeOku();

        var pasif = await Client.DeleteAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, pasif.StatusCode);

        var okunan = await Client.GetFromJsonAsync<UserPayload>($"/api/users/{created.Id}");
        Assert.False(okunan!.IsActive);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_pasiflestirmek_404_doner()
    {
        var response = await Client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Davetiye_yeniden_gonderme_204_doner()
    {
        var created = await KullaniciOlusturVeOku();

        var response = await Client.PostAsync($"/api/users/{created.Id}/resend-invitation", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kullaniciya_davetiye_gondermek_404_doner()
    {
        var response = await Client.PostAsync($"/api/users/{Guid.NewGuid()}/resend-invitation", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<UserPayload> KullaniciOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync("/api/users", YeniKullanici());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserPayload>())!;
    }

    private sealed record UserPayload(Guid Id, string Email, string? FirstName, string? LastName, bool EmailConfirmed, bool IsActive);
}
