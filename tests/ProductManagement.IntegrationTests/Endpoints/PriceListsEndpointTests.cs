using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// PriceLists uç noktalarının davranış sözleşmesi.
///
/// Fiyat listesi, kanal ve müşteri grubu bazlı fiyatlandırmanın taşıyıcısıdır; kalemleri
/// ürünlere bağlanır. Ürün/fiyat hattının üçüncü ve son çekirdek halkasıdır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class PriceListsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public PriceListsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/pricelists";
    private static string ItemsOf(Guid priceListId) => $"{BasePath}/{priceListId}/items";
    private static string ItemById(Guid itemId) => $"{BasePath}/items/{itemId}";

    // ── Fiyat listesi ─────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_kodu_sistem_uretir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniListe());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PriceListPayload>();
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.StartsWith("PL-", created.Code, StringComparison.OrdinalIgnoreCase);
    }

    [DockerFact]
    public async Task Olusturulan_liste_kimligiyle_geri_okunabilir()
    {
        var liste = await ListeOlustur();

        var response = await Client.GetAsync($"{BasePath}/{liste.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<PriceListPayload>();
        Assert.Equal(liste.Id, okunan!.Id);
    }

    [DockerFact]
    public async Task Olmayan_liste_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Liste_guncelleme_204_doner_ve_kalici_olur()
    {
        var liste = await ListeOlustur();
        var guncel = new UpdatePriceListPayload(
            Code: liste.Code,
            Name: "Güncellenmiş Liste",
            Description: "güncellendi",
            CurrencyCode: "EUR",
            IsActive: true,
            ValidFrom: null,
            ValidTo: null,
            SalesChannel: "web",
            CustomerGroupCode: "BAYI");

        var response = await Client.PutAsJsonAsync($"{BasePath}/{liste.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<PriceListPayload>($"{BasePath}/{liste.Id}");
        Assert.Equal("Güncellenmiş Liste", okunan!.Name);
        Assert.Equal("EUR", okunan.CurrencyCode);
        Assert.Equal("BAYI", okunan.CustomerGroupCode);
    }

    [DockerFact]
    public async Task Olmayan_listeyi_guncellemek_404_doner()
    {
        var guncel = new UpdatePriceListPayload("PL-YOK", "Ad", null, "TRY", true, null, null, null, null);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_liste_artik_bulunamaz()
    {
        var liste = await ListeOlustur();

        var silme = await Client.DeleteAsync($"{BasePath}/{liste.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{BasePath}/{liste.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    // ── Fiyat listesi kalemleri ───────────────────────────────────────────────

    [DockerFact]
    public async Task Yeni_listenin_kalemleri_bostur()
    {
        var liste = await ListeOlustur();

        var kalemler = await Client.GetFromJsonAsync<List<PriceListItemPayload>>(ItemsOf(liste.Id));

        Assert.Empty(kalemler!);
    }

    [DockerFact]
    public async Task Kalem_olusturma_201_doner()
    {
        var liste = await ListeOlustur();
        var urun = await TestData.UrunOlustur(Client);

        var response = await Client.PostAsJsonAsync($"{BasePath}/items", YeniKalem(liste.Id, urun.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PriceListItemPayload>();
        Assert.Equal(liste.Id, created!.ProductPriceListId);
        Assert.Equal(urun.Id, created.ProductId);
        Assert.Equal(149.90m, created.Amount);
    }

    [DockerFact]
    public async Task Olusturulan_kalem_listenin_kalemlerinde_gorunur()
    {
        var (liste, kalem) = await ListeVeKalem();

        var kalemler = await Client.GetFromJsonAsync<List<PriceListItemPayload>>(ItemsOf(liste.Id));

        Assert.Contains(kalemler!, x => x.Id == kalem.Id);
    }

    [DockerFact]
    public async Task Kalem_kimligiyle_geri_okunabilir()
    {
        var (_, kalem) = await ListeVeKalem();

        var response = await Client.GetAsync(ItemById(kalem.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<PriceListItemPayload>();
        Assert.Equal(kalem.Id, okunan!.Id);
    }

    [DockerFact]
    public async Task Olmayan_kalem_404_doner()
    {
        var response = await Client.GetAsync(ItemById(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Kalem_guncelleme_204_doner_ve_kalici_olur()
    {
        var (_, kalem) = await ListeVeKalem();
        var guncel = new UpdatePriceListItemPayload(
            Amount: 129.00m,
            CompareAtAmount: 199.00m,
            MinQuantity: 10,
            MaxQuantity: 50);

        var response = await Client.PutAsJsonAsync(ItemById(kalem.Id), guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<PriceListItemPayload>(ItemById(kalem.Id));
        Assert.Equal(129.00m, okunan!.Amount);
        Assert.Equal(199.00m, okunan.CompareAtAmount);
        Assert.Equal(10, okunan.MinQuantity);
    }

    [DockerFact]
    public async Task Silinen_kalem_artik_bulunamaz()
    {
        var (_, kalem) = await ListeVeKalem();

        var silme = await Client.DeleteAsync(ItemById(kalem.Id));
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync(ItemById(kalem.Id));
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Bir_listeye_birden_fazla_urun_kalemi_eklenebilir()
    {
        var liste = await ListeOlustur();
        var birinci = await TestData.UrunOlustur(Client);
        var ikinci = await TestData.UrunOlustur(Client);

        await KalemOlustur(liste.Id, birinci.Id);
        await KalemOlustur(liste.Id, ikinci.Id);

        var kalemler = await Client.GetFromJsonAsync<List<PriceListItemPayload>>(ItemsOf(liste.Id));

        Assert.Equal(2, kalemler!.Count);
    }

    [DockerFact]
    public async Task Bir_listenin_kalemi_baska_listede_gorunmez()
    {
        var (birinciListe, kalem) = await ListeVeKalem();
        var ikinciListe = await ListeOlustur();

        Assert.NotEqual(birinciListe.Id, ikinciListe.Id);

        var digerininKalemleri = await Client.GetFromJsonAsync<List<PriceListItemPayload>>(ItemsOf(ikinciListe.Id));

        Assert.DoesNotContain(digerininKalemleri!, x => x.Id == kalem.Id);
    }

    // ── yardımcılar ───────────────────────────────────────────────────────────

    private static CreatePriceListPayload YeniListe() => new(
        Code: null,
        Name: $"Test Listesi {Guid.NewGuid().ToString("N")[..6]}",
        Description: "Entegrasyon testi tarafından oluşturuldu",
        CurrencyCode: "TRY",
        IsActive: true,
        ValidFrom: null,
        ValidTo: null,
        SalesChannel: null,
        CustomerGroupCode: null);

    private static CreatePriceListItemPayload YeniKalem(Guid priceListId, Guid productId) => new(
        ProductPriceListId: priceListId,
        ProductId: productId,
        ProductVariantId: null,
        Amount: 149.90m,
        CompareAtAmount: null,
        MinQuantity: null,
        MaxQuantity: null);

    private async Task<PriceListPayload> ListeOlustur()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniListe());
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PriceListPayload>())!;
    }

    private async Task<PriceListItemPayload> KalemOlustur(Guid priceListId, Guid productId)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/items", YeniKalem(priceListId, productId));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PriceListItemPayload>())!;
    }

    private async Task<(PriceListPayload Liste, PriceListItemPayload Kalem)> ListeVeKalem()
    {
        var liste = await ListeOlustur();
        var urun = await TestData.UrunOlustur(Client);
        var kalem = await KalemOlustur(liste.Id, urun.Id);
        return (liste, kalem);
    }

    private sealed record CreatePriceListPayload(
        string? Code,
        string Name,
        string? Description,
        string CurrencyCode,
        bool IsActive,
        DateTime? ValidFrom,
        DateTime? ValidTo,
        string? SalesChannel,
        string? CustomerGroupCode);

    private sealed record UpdatePriceListPayload(
        string Code,
        string Name,
        string? Description,
        string CurrencyCode,
        bool IsActive,
        DateTime? ValidFrom,
        DateTime? ValidTo,
        string? SalesChannel,
        string? CustomerGroupCode);

    private sealed record PriceListPayload(
        Guid Id,
        string Code,
        string Name,
        string CurrencyCode,
        bool IsActive,
        string? SalesChannel,
        string? CustomerGroupCode);

    private sealed record CreatePriceListItemPayload(
        Guid ProductPriceListId,
        Guid ProductId,
        Guid? ProductVariantId,
        decimal Amount,
        decimal? CompareAtAmount,
        int? MinQuantity,
        int? MaxQuantity);

    private sealed record UpdatePriceListItemPayload(
        decimal Amount,
        decimal? CompareAtAmount,
        int? MinQuantity,
        int? MaxQuantity);

    private sealed record PriceListItemPayload(
        Guid Id,
        Guid ProductPriceListId,
        Guid ProductId,
        decimal Amount,
        decimal? CompareAtAmount,
        int? MinQuantity,
        int? MaxQuantity);
}
