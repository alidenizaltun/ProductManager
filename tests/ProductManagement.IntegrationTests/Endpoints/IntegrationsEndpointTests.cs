using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Entegrasyon (dış servis bağlantısı) uç noktalarının davranış sözleşmesi.
///
/// <c>POST /{id}/test</c> gerçek bir sağlayıcıya bağlanmaya çalıştığı için kapsam dışı
/// bırakıldı — burada yalnızca CRUD davranışı sınanır. Gerçek SQL Server gerektirir —
/// Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class IntegrationsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public IntegrationsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static object YeniEntegrasyon() => new
    {
        Name = $"Test Entegrasyon {Guid.NewGuid().ToString("N")[..6]}",
        Type = "Email",
        // ProviderKey benzersiz olmalı — StartupSeedService "Mailjet" ile seed ediyor, testler kendi
        // kayıtlarıyla da çakışmasın diye burada rastgele üretiliyor.
        ProviderKey = $"TestProvider_{Guid.NewGuid().ToString("N")[..8]}",
        IsEnabled = true,
    };

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/integrations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync("/api/integrations", YeniEntegrasyon());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [DockerFact]
    public async Task Bos_ad_400_doner()
    {
        var gecersiz = new { Name = "", Type = "Email", ProviderKey = "TestProvider" };

        var response = await Client.PostAsJsonAsync("/api/integrations", gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await EntegrasyonOlusturVeOku();

        var response = await Client.GetAsync($"/api/integrations/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/integrations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await EntegrasyonOlusturVeOku();
        var guncel = new { Name = "Güncellenmiş Entegrasyon", IsEnabled = false };

        var response = await Client.PutAsJsonAsync($"/api/integrations/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<IntegrationPayload>($"/api/integrations/{created.Id}");
        Assert.Equal("Güncellenmiş Entegrasyon", okunan!.Name);
        Assert.False(okunan.IsEnabled);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new { Name = "Ad", IsEnabled = true };

        var response = await Client.PutAsJsonAsync($"/api/integrations/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_kayit_artik_bulunamaz()
    {
        var created = await EntegrasyonOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/integrations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/integrations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_silmek_404_doner()
    {
        var response = await Client.DeleteAsync($"/api/integrations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<IntegrationPayload> EntegrasyonOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync("/api/integrations", YeniEntegrasyon());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IntegrationPayload>())!;
    }

    private sealed record IntegrationPayload(Guid Id, string Name, string Type, string ProviderKey, bool IsEnabled);
}
