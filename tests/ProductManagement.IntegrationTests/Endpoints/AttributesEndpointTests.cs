using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Attributes (ürün öznitelik tanımı) uç noktalarının davranış sözleşmesi.
///
/// Bu controller'da kod üretimi yok — <c>Key</c> alanı doğrudan istemciden gelir.
/// Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class AttributesEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public AttributesEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/attributes";

    private static CreateAttributePayload YeniOznitelik(string? key = null, int dataType = 1) => new(
        Key: key ?? $"test_ozellik_{Guid.NewGuid().ToString("N")[..6]}",
        DisplayName: "Test Özelliği",
        DataType: dataType,
        IsRequired: false,
        IsFilterable: false,
        IsVariantAxis: false,
        AllowedValuesJson: null,
        ValidationRuleJson: null);

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniOznitelik());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<AttributePayload>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [DockerFact]
    public async Task Bos_anahtar_400_doner()
    {
        var gecersiz = YeniOznitelik() with { Key = "" };

        var response = await Client.PostAsJsonAsync(BasePath, gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Gecersiz_veri_tipi_400_doner()
    {
        var gecersiz = YeniOznitelik() with { DataType = 99 };

        var response = await Client.PostAsJsonAsync(BasePath, gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await OlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<AttributePayload>();
        Assert.Equal(created.Id, okunan!.Id);
        Assert.Equal(created.Key, okunan.Key);
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
        var guncel = new UpdateAttributePayload("anahtar", "Ad", 1, false, false, false, null, null);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await OlusturVeOku();
        var guncel = new UpdateAttributePayload(
            Key: created.Key,
            DisplayName: "Güncellenmiş Görünen Ad",
            DataType: 2,
            IsRequired: true,
            IsFilterable: true,
            IsVariantAxis: false,
            AllowedValuesJson: null,
            ValidationRuleJson: null);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<AttributePayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Görünen Ad", okunan!.DisplayName);
        Assert.True(okunan.IsRequired);
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

    private async Task<AttributePayload> OlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniOznitelik());
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<AttributePayload>();
        return created!;
    }

    private sealed record CreateAttributePayload(
        string Key,
        string DisplayName,
        int DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantAxis,
        string? AllowedValuesJson,
        string? ValidationRuleJson);

    private sealed record UpdateAttributePayload(
        string Key,
        string DisplayName,
        int DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantAxis,
        string? AllowedValuesJson,
        string? ValidationRuleJson);

    private sealed record AttributePayload(
        Guid Id,
        string Key,
        string DisplayName,
        int DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantAxis,
        string? AllowedValuesJson,
        string? ValidationRuleJson);
}
