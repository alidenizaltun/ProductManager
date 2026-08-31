using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Fiyat şablonu uç noktalarının davranış sözleşmesi.
///
/// <c>apply</c>/<c>apply-bulk</c> fiyat motorunun kural üretme mantığına giriyor ve kapsam
/// dışı bırakıldı — burada CRUD ve kullanım listesi sınanır. Gerçek SQL Server gerektirir —
/// Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class PricingTemplatesEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public PricingTemplatesEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/pricing-templates";

    private static object YeniSablon() => new
    {
        Name = $"Test Şablon {Guid.NewGuid().ToString("N")[..6]}",
        TemplateKind = 1,
        CurrencyCode = "TRY",
        PayloadJson = "{\"type\":\"percent\",\"value\":10}",
        IsActive = true,
    };

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_kod_uretir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniSablon());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PricingTemplatePayload>();
        Assert.StartsWith("TPL-", created!.Code);
    }

    [DockerFact]
    public async Task Payload_olmadan_400_doner()
    {
        var gecersiz = new { Name = "Şablon", TemplateKind = 1, CurrencyCode = "TRY" };

        var response = await Client.PostAsJsonAsync(BasePath, gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await SablonOlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Kullanimlar_bos_liste_200_doner()
    {
        var created = await SablonOlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}/usages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var usages = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.Empty(usages!);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await SablonOlusturVeOku();
        var guncel = new
        {
            Code = created.Code,
            Name = "Güncellenmiş Şablon",
            CurrencyCode = "TRY",
            PayloadJson = "{\"type\":\"percent\",\"value\":15}",
            IsActive = true,
        };

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<PricingTemplatePayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Şablon", okunan!.Name);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new { Code = "TPL-000000", Name = "Ad", CurrencyCode = "TRY", PayloadJson = "{}", IsActive = true };

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_kayit_artik_bulunamaz()
    {
        var created = await SablonOlusturVeOku();

        var silme = await Client.DeleteAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_silmek_404_doner()
    {
        var response = await Client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<PricingTemplatePayload> SablonOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniSablon());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PricingTemplatePayload>())!;
    }

    private sealed record PricingTemplatePayload(Guid Id, string Code, string Name, int TemplateKind, string CurrencyCode, bool IsActive);
}
