using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Ürün profili (fiziksel/yazılım/hizmet/abonelik) uç noktalarının davranış sözleşmesi.
///
/// Dört profil tipi de aynı upsert deseni izler: kayıt yoksa GET 404 verir, PUT hem
/// oluşturur hem günceller (upsert), DELETE kaldırır. Her profil bir ürüne bağlıdır —
/// ürün oluşturmak ön koşuldur. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ProductProfilesEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public ProductProfilesEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    // ── Fiziksel profil ──────────────────────────────────────────────────────

    [DockerFact]
    public async Task Fiziksel_profil_yokken_404_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync($"/api/products/{urun.Id}/profiles/physical");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Fiziksel_profil_upsert_olusturur_ve_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { Weight = 1.5m, RequiresShipping = true, IsFragile = true, RequiresSerialNumber = false };

        var upsert = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/profiles/physical", payload);
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var okunan = await Client.GetFromJsonAsync<PhysicalProfilePayload>($"/api/products/{urun.Id}/profiles/physical");
        Assert.Equal(1.5m, okunan!.Weight);
        Assert.True(okunan.IsFragile);
    }

    [DockerFact]
    public async Task Fiziksel_profil_silinen_kayit_artik_bulunamaz()
    {
        var urun = await TestData.UrunOlustur(Client);
        await Client.PutAsJsonAsync($"/api/products/{urun.Id}/profiles/physical", new { RequiresShipping = true });

        var silme = await Client.DeleteAsync($"/api/products/{urun.Id}/profiles/physical");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/{urun.Id}/profiles/physical");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Fiziksel_profil_olmayan_kaydi_silmek_404_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.DeleteAsync($"/api/products/{urun.Id}/profiles/physical");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Yazılım profili ──────────────────────────────────────────────────────

    [DockerFact]
    public async Task Yazilim_profil_upsert_olusturur_ve_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { Version = "1.0.0", DownloadUrl = "https://example.invalid/build.zip" };

        var upsert = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/profiles/software", payload);
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var okunan = await Client.GetFromJsonAsync<SoftwareProfilePayload>($"/api/products/{urun.Id}/profiles/software");
        Assert.Equal("1.0.0", okunan!.Version);
    }

    // ── Hizmet profili ───────────────────────────────────────────────────────

    [DockerFact]
    public async Task Hizmet_profil_upsert_olusturur_ve_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { DeliveryMode = 2, DurationInMinutes = 60 };

        var upsert = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/profiles/service", payload);
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var okunan = await Client.GetFromJsonAsync<ServiceProfilePayload>($"/api/products/{urun.Id}/profiles/service");
        Assert.Equal(60, okunan!.DurationInMinutes);
    }

    // ── Abonelik profili ─────────────────────────────────────────────────────

    [DockerFact]
    public async Task Abonelik_profil_upsert_olusturur_ve_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { BillingPeriodUnit = 3, BillingPeriodValue = 1, AutoRenew = true };

        var upsert = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/profiles/subscription", payload);
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var okunan = await Client.GetFromJsonAsync<SubscriptionProfilePayload>($"/api/products/{urun.Id}/profiles/subscription");
        Assert.True(okunan!.AutoRenew);
    }

    private sealed record PhysicalProfilePayload(Guid Id, Guid ProductId, decimal? Weight, bool IsFragile);
    private sealed record SoftwareProfilePayload(Guid Id, Guid ProductId, string? Version);
    private sealed record ServiceProfilePayload(Guid Id, Guid ProductId, int DeliveryMode, int? DurationInMinutes);
    private sealed record SubscriptionProfilePayload(Guid Id, Guid ProductId, bool AutoRenew);
}
