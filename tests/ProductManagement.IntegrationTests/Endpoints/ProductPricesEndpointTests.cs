using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// ProductCommerce fiyat uçlarının davranış sözleşmesi.
///
/// Fiyat, PM'nin en çok tüketilen verisidir: B2B vitrini ve bayi paneli fiyatı buradan
/// türetir. Bu yüzden ürün/fiyat hattının ikinci halkası burasıdır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ProductPricesEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public ProductPricesEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static string PricesOf(Guid productId) => $"/api/products/{productId}/prices";
    private static string PriceById(Guid priceId) => $"/api/products/prices/{priceId}";

    private static CreatePricePayload YeniFiyat(Guid productId, decimal tutar = 199.90m) => new(
        ProductId: productId,
        ProductVariantId: null,
        RegionId: null,
        PriceType: 1,
        Amount: tutar,
        CompareAtAmount: null,
        CurrencyCode: "TRY",
        MinQuantity: null,
        MaxQuantity: null,
        ValidFrom: null,
        ValidTo: null,
        SalesChannel: null,
        CustomerGroupCode: null);

    [DockerFact]
    public async Task Urunun_fiyat_listesi_200_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync(PricesOf(urun.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Yeni_urunun_fiyat_listesi_bostur()
    {
        var urun = await TestData.UrunOlustur(Client);

        var fiyatlar = await Client.GetFromJsonAsync<List<PricePayload>>(PricesOf(urun.Id));

        Assert.Empty(fiyatlar!);
    }

    [DockerFact]
    public async Task Fiyat_olusturma_201_doner()
    {
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.PostAsJsonAsync(PricesOf(urun.Id), YeniFiyat(urun.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PricePayload>();
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(urun.Id, created.ProductId);
        Assert.Equal(199.90m, created.Amount);
    }

    [DockerFact]
    public async Task Olusturulan_fiyat_urunun_listesinde_gorunur()
    {
        var urun = await TestData.UrunOlustur(Client);
        var fiyat = await FiyatOlustur(urun.Id);

        var fiyatlar = await Client.GetFromJsonAsync<List<PricePayload>>(PricesOf(urun.Id));

        Assert.Contains(fiyatlar!, x => x.Id == fiyat.Id);
    }

    [DockerFact]
    public async Task Fiyat_kimligiyle_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var fiyat = await FiyatOlustur(urun.Id);

        var response = await Client.GetAsync(PriceById(fiyat.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<PricePayload>();
        Assert.Equal(fiyat.Id, okunan!.Id);
    }

    [DockerFact]
    public async Task Olmayan_fiyat_404_doner()
    {
        var response = await Client.GetAsync(PriceById(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Fiyat_guncelleme_204_doner_ve_kalici_olur()
    {
        var urun = await TestData.UrunOlustur(Client);
        var fiyat = await FiyatOlustur(urun.Id);

        var guncel = new UpdatePricePayload(
            ProductVariantId: null,
            RegionId: null,
            PriceType: 1,
            Amount: 249.50m,
            CompareAtAmount: 299.00m,
            CurrencyCode: "USD",
            MinQuantity: 5,
            MaxQuantity: 100,
            ValidFrom: null,
            ValidTo: null,
            SalesChannel: null,
            CustomerGroupCode: null);

        var response = await Client.PutAsJsonAsync(PriceById(fiyat.Id), guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<PricePayload>(PriceById(fiyat.Id));
        Assert.Equal(249.50m, okunan!.Amount);
        Assert.Equal(299.00m, okunan.CompareAtAmount);
        Assert.Equal("USD", okunan.CurrencyCode);
        Assert.Equal(5, okunan.MinQuantity);
    }

    [DockerFact]
    public async Task Olmayan_fiyati_guncellemek_404_doner()
    {
        var guncel = new UpdatePricePayload(
            null, null, 1, 10m, null, "TRY", null, null, null, null, null, null);

        var response = await Client.PutAsJsonAsync(PriceById(Guid.NewGuid()), guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_fiyat_artik_bulunamaz()
    {
        var urun = await TestData.UrunOlustur(Client);
        var fiyat = await FiyatOlustur(urun.Id);

        var silme = await Client.DeleteAsync(PriceById(fiyat.Id));
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync(PriceById(fiyat.Id));
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_fiyati_silmek_404_doner()
    {
        var response = await Client.DeleteAsync(PriceById(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Bir_urune_birden_fazla_fiyat_eklenebilir()
    {
        // Kademeli fiyatlandırma (miktar aralıkları) bu davranışa dayanır.
        var urun = await TestData.UrunOlustur(Client);
        await FiyatOlustur(urun.Id, 100m);
        await FiyatOlustur(urun.Id, 90m);

        var fiyatlar = await Client.GetFromJsonAsync<List<PricePayload>>(PricesOf(urun.Id));

        Assert.Equal(2, fiyatlar!.Count);
    }

    [DockerFact]
    public async Task Bir_urunun_fiyati_baska_urunun_listesinde_gorunmez()
    {
        var birinci = await TestData.UrunOlustur(Client);
        var ikinci = await TestData.UrunOlustur(Client);
        var fiyat = await FiyatOlustur(birinci.Id);

        var digerininFiyatlari = await Client.GetFromJsonAsync<List<PricePayload>>(PricesOf(ikinci.Id));

        Assert.DoesNotContain(digerininFiyatlari!, x => x.Id == fiyat.Id);
    }

    private async Task<PricePayload> FiyatOlustur(Guid productId, decimal tutar = 199.90m)
    {
        var response = await Client.PostAsJsonAsync(PricesOf(productId), YeniFiyat(productId, tutar));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PricePayload>())!;
    }

    private sealed record CreatePricePayload(
        Guid ProductId,
        Guid? ProductVariantId,
        Guid? RegionId,
        int PriceType,
        decimal Amount,
        decimal? CompareAtAmount,
        string CurrencyCode,
        int? MinQuantity,
        int? MaxQuantity,
        DateTime? ValidFrom,
        DateTime? ValidTo,
        string? SalesChannel,
        string? CustomerGroupCode);

    private sealed record UpdatePricePayload(
        Guid? ProductVariantId,
        Guid? RegionId,
        int PriceType,
        decimal Amount,
        decimal? CompareAtAmount,
        string CurrencyCode,
        int? MinQuantity,
        int? MaxQuantity,
        DateTime? ValidFrom,
        DateTime? ValidTo,
        string? SalesChannel,
        string? CustomerGroupCode);

    private sealed record PricePayload(
        Guid Id,
        Guid ProductId,
        Guid? ProductVariantId,
        int PriceType,
        decimal Amount,
        decimal? CompareAtAmount,
        string CurrencyCode,
        int? MinQuantity,
        int? MaxQuantity);
}
