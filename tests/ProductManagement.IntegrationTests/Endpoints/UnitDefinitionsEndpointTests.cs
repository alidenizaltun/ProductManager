using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// UnitDefinitions uç noktalarının davranış sözleşmesi.
///
/// Plandaki asgari senaryo setinin PM tarafındaki şablonu: başarı, bulunamadı,
/// tam yaşam döngüsü, filtre ve <b>sistem tarafından üretilen kod</b> kuralı.
/// Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class UnitDefinitionsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public UnitDefinitionsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    /// <summary>İstemci geç kurulur: Docker yokken fabrikaya hiç dokunulmaz.</summary>
    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/unit-definitions";

    private static CreateUnitDefinitionPayload YeniBirim(string? kod = null, bool aktif = true) => new(
        Code: kod,
        Name: $"Test Birimi {Guid.NewGuid().ToString("N")[..6]}",
        Description: "Entegrasyon testi tarafından oluşturuldu",
        IsActive: aktif,
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
        var response = await Client.PostAsJsonAsync(BasePath, YeniBirim());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [DockerFact]
    public async Task Kod_verilmezse_sistem_uretir()
    {
        // Ekleme ekranlarında kod sorulmaz; backend "<ÖNEK>-000001" biçiminde üretir.
        var response = await Client.PostAsJsonAsync(BasePath, YeniBirim(kod: null));
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>();

        Assert.False(string.IsNullOrWhiteSpace(created!.Code));
        Assert.StartsWith("UNIT-", created.Code, StringComparison.OrdinalIgnoreCase);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await OlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>();
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
        var guncel = new UpdateUnitDefinitionPayload("KOD", "Ad", null, true, 0);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await OlusturVeOku();
        var guncel = new UpdateUnitDefinitionPayload(
            Code: created.Code,
            Name: "Güncellenmiş Birim",
            Description: "güncellendi",
            IsActive: true,
            SortOrder: 42);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<UnitDefinitionPayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Birim", okunan!.Name);
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

    [DockerFact]
    public async Task Pasif_kayitlar_varsayilan_listede_gelmez_ama_includeInactive_ile_gelir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniBirim(aktif: false));
        response.EnsureSuccessStatusCode();
        var pasif = await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>();

        var varsayilan = await Client.GetFromJsonAsync<List<UnitDefinitionPayload>>(BasePath);
        var tumu = await Client.GetFromJsonAsync<List<UnitDefinitionPayload>>($"{BasePath}?includeInactive=true");

        Assert.DoesNotContain(varsayilan!, x => x.Id == pasif!.Id);
        Assert.Contains(tumu!, x => x.Id == pasif!.Id);
    }

    private async Task<UnitDefinitionPayload> OlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniBirim());
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>();
        return created!;
    }

    // Sözleşmeyi testin kendisi tanımlar: üretim DTO'su sessizce değişirse test fark eder.
    private sealed record CreateUnitDefinitionPayload(
        string? Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);

    private sealed record UpdateUnitDefinitionPayload(
        string Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);

    private sealed record UnitDefinitionPayload(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        bool IsActive,
        int SortOrder);
}
