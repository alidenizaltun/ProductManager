using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Yazılım lisanslama uç noktalarının (modül, modül-teklif fiyatı, lisans teklifi)
/// davranış sözleşmesi. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class SoftwareLicenseEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public SoftwareLicenseEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    // ── Modüller ─────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Modul_olusturma_201_doner_ve_listede_gorunur()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/modules", YeniModul());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var liste = await Client.GetFromJsonAsync<List<ModulePayload>>($"/api/products/{urun.Id}/modules");
        Assert.Single(liste!);
    }

    [DockerFact]
    public async Task Modul_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var (urun, modul) = await ModulOlusturVeOku();
        var guncel = new { ModuleCode = modul.ModuleCode, Name = "Güncellenmiş Modül", IsOptional = true, IsActive = true, SortOrder = 1 };

        var response = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/modules/{modul.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<ModulePayload>($"/api/products/{urun.Id}/modules/{modul.Id}");
        Assert.Equal("Güncellenmiş Modül", okunan!.Name);
    }

    [DockerFact]
    public async Task Modul_silinen_kayit_artik_bulunamaz()
    {
        var (urun, modul) = await ModulOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/products/{urun.Id}/modules/{modul.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/{urun.Id}/modules/{modul.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Modul_olmayan_kimlik_404_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync($"/api/products/{urun.Id}/modules/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static object YeniModul() => new
    {
        ModuleCode = $"MOD-{Guid.NewGuid().ToString("N")[..6]}",
        Name = "Test Modülü",
        IsOptional = true,
        IsActive = true,
    };

    private async Task<(TestData.ProductPayload Urun, ModulePayload Modul)> ModulOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/modules", YeniModul());
        response.EnsureSuccessStatusCode();
        var modul = (await response.Content.ReadFromJsonAsync<ModulePayload>())!;
        return (urun, modul);
    }

    // ── Lisans teklifleri ────────────────────────────────────────────────────

    [DockerFact]
    public async Task Lisans_teklifi_olusturma_201_doner_ve_listede_gorunur()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/license-offerings", YeniLisansTeklifi());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var liste = await Client.GetFromJsonAsync<List<LicenseOfferingPayload>>($"/api/products/{urun.Id}/license-offerings");
        Assert.Single(liste!);
    }

    [DockerFact]
    public async Task Lisans_teklifi_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var (urun, teklif) = await LisansTeklifiOlusturVeOku();
        var guncel = new { LicenseModel = 1, Name = "Güncellenmiş Teklif", BasePrice = 200m, CurrencyCode = "TRY", AutoRenew = true, IsActive = true, SortOrder = 0 };

        var response = await Client.PutAsJsonAsync($"/api/products/{urun.Id}/license-offerings/{teklif.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<LicenseOfferingPayload>($"/api/products/{urun.Id}/license-offerings/{teklif.Id}");
        Assert.Equal("Güncellenmiş Teklif", okunan!.Name);
    }

    [DockerFact]
    public async Task Lisans_teklifi_silinen_kayit_artik_bulunamaz()
    {
        var (urun, teklif) = await LisansTeklifiOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/products/{urun.Id}/license-offerings/{teklif.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/{urun.Id}/license-offerings/{teklif.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private static object YeniLisansTeklifi() => new
    {
        LicenseModel = 1,
        Name = "Standart",
        BasePrice = 100m,
        CurrencyCode = "TRY",
        AutoRenew = true,
        IsActive = true,
    };

    private async Task<(TestData.ProductPayload Urun, LicenseOfferingPayload Teklif)> LisansTeklifiOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/license-offerings", YeniLisansTeklifi());
        response.EnsureSuccessStatusCode();
        var teklif = (await response.Content.ReadFromJsonAsync<LicenseOfferingPayload>())!;
        return (urun, teklif);
    }

    // ── Modül teklif fiyatları ───────────────────────────────────────────────

    [DockerFact]
    public async Task Modul_teklif_fiyati_olusturma_201_doner()
    {
        var (urun, modul, teklif) = await ModulVeTeklifOlusturVeOku();
        var payload = new { ProductModuleId = modul.Id, ProductLicenseOfferingId = teklif.Id, Price = 50m, CurrencyCode = "TRY", IsActive = true };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/modules/{modul.Id}/offering-prices", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OfferingPricePayload>();
        Assert.Equal(50m, created!.Price);
    }

    [DockerFact]
    public async Task Modul_teklif_fiyati_silinen_kayit_artik_bulunamaz()
    {
        var (urun, modul, teklif) = await ModulVeTeklifOlusturVeOku();
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/modules/{modul.Id}/offering-prices",
            new { ProductModuleId = modul.Id, ProductLicenseOfferingId = teklif.Id, Price = 50m, CurrencyCode = "TRY", IsActive = true });
        var created = await olusturma.Content.ReadFromJsonAsync<OfferingPricePayload>();

        var silme = await Client.DeleteAsync($"/api/products/{urun.Id}/modules/{modul.Id}/offering-prices/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);
    }

    private async Task<(TestData.ProductPayload Urun, ModulePayload Modul, LicenseOfferingPayload Teklif)> ModulVeTeklifOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);

        var modulResponse = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/modules", YeniModul());
        modulResponse.EnsureSuccessStatusCode();
        var modul = (await modulResponse.Content.ReadFromJsonAsync<ModulePayload>())!;

        var teklifResponse = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/license-offerings", YeniLisansTeklifi());
        teklifResponse.EnsureSuccessStatusCode();
        var teklif = (await teklifResponse.Content.ReadFromJsonAsync<LicenseOfferingPayload>())!;

        return (urun, modul, teklif);
    }

    private sealed record ModulePayload(Guid Id, Guid ProductId, string ModuleCode, string Name);
    private sealed record LicenseOfferingPayload(Guid Id, Guid ProductId, string Name, decimal BasePrice);
    private sealed record OfferingPricePayload(Guid Id, Guid ProductModuleId, Guid ProductLicenseOfferingId, decimal Price);
}
