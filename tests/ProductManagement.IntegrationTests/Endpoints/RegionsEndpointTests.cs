using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Regions uç noktalarının davranış sözleşmesi.
///
/// UnitDefinitions ile aynı iskelet: başarı, doğrulama hatası, bulunamadı, tam yaşam
/// döngüsü ve sistem tarafından üretilen kod kuralı. Gerçek SQL Server gerektirir —
/// Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RegionsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public RegionsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/regions";

    private static CreateRegionPayload YeniBolge(string? kod = null) => new(
        Code: kod,
        Name: $"Test Bölgesi {Guid.NewGuid().ToString("N")[..6]}",
        Description: "Entegrasyon testi tarafından oluşturuldu",
        IsActive: true,
        SortOrder: 0);

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniBolge());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<RegionPayload>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [DockerFact]
    public async Task Kod_verilmezse_sistem_uretir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniBolge(kod: null));
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<RegionPayload>();

        Assert.False(string.IsNullOrWhiteSpace(created!.Code));
        Assert.StartsWith("REG-", created.Code, StringComparison.OrdinalIgnoreCase);
    }

    [DockerFact]
    public async Task Bos_ad_400_doner()
    {
        var gecersiz = YeniBolge() with { Name = "" };

        var response = await Client.PostAsJsonAsync(BasePath, gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await OlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<RegionPayload>();
        Assert.Equal(created.Id, okunan!.Id);
        Assert.Equal(created.Name, okunan.Name);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new UpdateRegionPayload("KOD", "Ad", null, true, 0);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await OlusturVeOku();
        var guncel = new UpdateRegionPayload(
            Code: created.Code,
            Name: "Güncellenmiş Bölge",
            Description: "güncellendi",
            IsActive: true,
            SortOrder: 42);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RegionPayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Bölge", okunan!.Name);
        Assert.Equal(42, okunan.SortOrder);
    }

    [DockerFact]
    public async Task Silinen_kayit_artik_bulunamaz()
    {
        var created = await OlusturVeOku();

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

    private async Task<RegionPayload> OlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniBolge());
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<RegionPayload>();
        return created!;
    }

    private sealed record CreateRegionPayload(
        string? Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);

    private sealed record UpdateRegionPayload(
        string Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);

    private sealed record RegionPayload(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);
}
