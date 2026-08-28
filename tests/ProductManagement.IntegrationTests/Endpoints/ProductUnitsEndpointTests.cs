using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// ProductCommerce birim uçlarının davranış sözleşmesi.
///
/// Ürün birimleri, bir birim tanımına (<c>/api/unit-definitions</c>) bağlanır ve
/// fiyat/stok hesaplarının çarpanını belirler. Bu yüzden ürün/fiyat hattının parçasıdır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ProductUnitsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public ProductUnitsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static string UnitsOf(Guid productId) => $"/api/products/{productId}/units";
    private static string UnitById(Guid productUnitId) => $"/api/products/units/{productUnitId}";

    [DockerFact]
    public async Task Urunun_birim_listesi_200_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync(UnitsOf(urun.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Birim_olusturma_201_doner()
    {
        var (urun, tanim) = await OnKosul();

        var response = await Client.PostAsJsonAsync(UnitsOf(urun.Id), YeniBirim(urun.Id, tanim.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductUnitPayload>();
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(urun.Id, created.ProductId);
        Assert.Equal(tanim.Id, created.UnitDefinitionId);
    }

    [DockerFact]
    public async Task Olusturulan_birim_urunun_listesinde_gorunur()
    {
        var (urun, tanim) = await OnKosul();
        var birim = await BirimOlustur(urun.Id, tanim.Id);

        var birimler = await Client.GetFromJsonAsync<List<ProductUnitPayload>>(UnitsOf(urun.Id));

        Assert.Contains(birimler!, x => x.Id == birim.Id);
    }

    [DockerFact]
    public async Task Birim_kimligiyle_geri_okunabilir()
    {
        var (urun, tanim) = await OnKosul();
        var birim = await BirimOlustur(urun.Id, tanim.Id);

        var response = await Client.GetAsync(UnitById(birim.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<ProductUnitPayload>();
        Assert.Equal(birim.Id, okunan!.Id);
    }

    [DockerFact]
    public async Task Olmayan_birim_404_doner()
    {
        var response = await Client.GetAsync(UnitById(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Birim_guncelleme_204_doner_ve_kalici_olur()
    {
        var (urun, tanim) = await OnKosul();
        var birim = await BirimOlustur(urun.Id, tanim.Id);

        var guncel = new UpdateProductUnitPayload(
            UnitDefinitionId: tanim.Id,
            Code: "KOLI",
            Name: "Koli",
            Description: "güncellendi",
            Role: 2,
            IsDefault: true,
            IsActive: true,
            SortOrder: 5);

        var response = await Client.PutAsJsonAsync(UnitById(birim.Id), guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<ProductUnitPayload>(UnitById(birim.Id));
        Assert.Equal("Koli", okunan!.Name);
        Assert.Equal("KOLI", okunan.Code);
        Assert.True(okunan.IsDefault);
        Assert.Equal(5, okunan.SortOrder);
    }

    [DockerFact]
    public async Task Olmayan_birimi_guncellemek_404_doner()
    {
        var tanim = await TestData.BirimTanimiOlustur(Client);
        var guncel = new UpdateProductUnitPayload(tanim.Id, "KOD", "Ad", null, 1, false, true, 0);

        var response = await Client.PutAsJsonAsync(UnitById(Guid.NewGuid()), guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_birim_artik_bulunamaz()
    {
        var (urun, tanim) = await OnKosul();
        var birim = await BirimOlustur(urun.Id, tanim.Id);

        var silme = await Client.DeleteAsync(UnitById(birim.Id));
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync(UnitById(birim.Id));
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_birimi_silmek_404_doner()
    {
        var response = await Client.DeleteAsync(UnitById(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Bir_urune_birden_fazla_birim_eklenebilir()
    {
        // Adet / kutu / koli gibi çoklu birim ataması bu davranışa dayanır.
        var (urun, tanim) = await OnKosul();
        var digerTanim = await TestData.BirimTanimiOlustur(Client);

        await BirimOlustur(urun.Id, tanim.Id, kod: "ADET");
        await BirimOlustur(urun.Id, digerTanim.Id, kod: "KUTU");

        var birimler = await Client.GetFromJsonAsync<List<ProductUnitPayload>>(UnitsOf(urun.Id));

        Assert.Equal(2, birimler!.Count);
    }

    // ── yardımcılar ───────────────────────────────────────────────────────────

    private async Task<(TestData.ProductPayload Urun, TestData.UnitDefinitionPayload Tanim)> OnKosul()
    {
        var urun = await TestData.UrunOlustur(Client);
        var tanim = await TestData.BirimTanimiOlustur(Client);
        return (urun, tanim);
    }

    private static CreateProductUnitPayload YeniBirim(Guid productId, Guid unitDefinitionId, string kod = "ADET") => new(
        ProductId: productId,
        UnitDefinitionId: unitDefinitionId,
        Code: kod,
        Name: "Adet",
        Description: "Entegrasyon testi tarafından oluşturuldu",
        Role: 1,
        IsDefault: false,
        IsActive: true,
        SortOrder: 0);

    private async Task<ProductUnitPayload> BirimOlustur(Guid productId, Guid unitDefinitionId, string kod = "ADET")
    {
        var response = await Client.PostAsJsonAsync(UnitsOf(productId), YeniBirim(productId, unitDefinitionId, kod));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductUnitPayload>())!;
    }

    private sealed record CreateProductUnitPayload(
        Guid ProductId,
        Guid UnitDefinitionId,
        string Code,
        string Name,
        string? Description,
        int Role,
        bool IsDefault,
        bool IsActive,
        int SortOrder);

    private sealed record UpdateProductUnitPayload(
        Guid UnitDefinitionId,
        string Code,
        string Name,
        string? Description,
        int Role,
        bool IsDefault,
        bool IsActive,
        int SortOrder);

    private sealed record ProductUnitPayload(
        Guid Id,
        Guid ProductId,
        Guid UnitDefinitionId,
        string Code,
        string Name,
        int Role,
        bool IsDefault,
        bool IsActive,
        int SortOrder);
}
