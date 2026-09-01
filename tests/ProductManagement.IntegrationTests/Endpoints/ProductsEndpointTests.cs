using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Products uç noktalarının davranış sözleşmesi.
///
/// Ürün, PM'nin çekirdek varlığıdır: fiyat, birim, varyant ve kural uçlarının tamamı
/// buna asılıdır. Bu yüzden ürün/fiyat testlerinin ilk halkası burasıdır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ProductsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public ProductsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/products";

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, TestData.YeniUrun());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<TestData.ProductPayload>();
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [DockerFact]
    public async Task Kod_verilmezse_sistem_uretir()
    {
        // Ekleme ekranlarında kod sorulmaz; backend "<ÖNEK>-000001" biçiminde üretir.
        var created = await TestData.UrunOlustur(Client);

        Assert.False(string.IsNullOrWhiteSpace(created.ProductCode));
        Assert.StartsWith("PRD-", created.ProductCode, StringComparison.OrdinalIgnoreCase);
    }

    [DockerFact]
    public async Task Uretilen_kodlar_benzersizdir()
    {
        var birinci = await TestData.UrunOlustur(Client);
        var ikinci = await TestData.UrunOlustur(Client);

        Assert.NotEqual(birinci.ProductCode, ikinci.ProductCode);
    }

    [DockerFact]
    public async Task Olusturulan_urun_kimligiyle_geri_okunabilir()
    {
        var created = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<TestData.ProductPayload>();
        Assert.Equal(created.Id, okunan!.Id);
        Assert.Equal(created.Name, okunan.Name);
    }

    [DockerFact]
    public async Task Detay_ucu_200_doner()
    {
        var created = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync($"{BasePath}/{created.Id}/detail");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_urunun_detayi_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}/detail");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await TestData.UrunOlustur(Client);
        var guncel = new UpdateProductPayload(
            ProductCode: created.ProductCode,
            Name: "Güncellenmiş Ürün",
            ShortDescription: "güncellendi",
            Kind: 1,
            Status: 1,
            IsActive: true,
            IsSellable: false,
            IsPurchasable: true,
            TrackInventory: true,
            DefaultCurrencyCode: "USD",
            TaxRate: 10m);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<TestData.ProductPayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Ürün", okunan!.Name);
        Assert.Equal("USD", okunan.DefaultCurrencyCode);
        Assert.False(okunan.IsSellable);
    }

    [DockerFact]
    public async Task Olmayan_urunu_guncellemek_404_doner()
    {
        var guncel = new UpdateProductPayload(
            "PRD-YOK", "Ad", null, 1, 1, true, true, true, true, "TRY", null);

        var response = await Client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_urun_artik_bulunamaz()
    {
        var created = await TestData.UrunOlustur(Client);

        var silme = await Client.DeleteAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_urunu_silmek_404_doner()
    {
        var response = await Client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Arama_filtresi_ada_gore_daraltir()
    {
        var isaret = $"Filtre{Guid.NewGuid().ToString("N")[..8]}";
        var hedef = await TestData.UrunOlustur(Client, ad: isaret);
        await TestData.UrunOlustur(Client);

        var sonuc = await Client.GetFromJsonAsync<List<TestData.ProductPayload>>(
            $"{BasePath}?search={Uri.EscapeDataString(isaret)}");

        Assert.Contains(sonuc!, x => x.Id == hedef.Id);
        Assert.All(sonuc!, x => Assert.Contains(isaret, x.Name, StringComparison.OrdinalIgnoreCase));
    }

    [DockerFact]
    public async Task Tam_olusturma_gecersiz_ic_ice_modulle_400_doner()
    {
        // Faz 3: CreateProductFullRequestDto ve iç içe DTO'ları (Modules dahil) önceden
        // hiç doğrulanmıyordu. Boş ModuleCode/Name artık CreateProductModuleRequestDtoValidator
        // tarafından reddedilmeli.
        var payload = new
        {
            product = new
            {
                name = $"Tam Ürün {Guid.NewGuid().ToString("N")[..6]}",
                kind = 2,
                status = 1,
                isActive = true,
                isSellable = true,
                isPurchasable = true,
                trackInventory = false,
                defaultCurrencyCode = "TRY"
            },
            modules = new[]
            {
                new { moduleCode = "", name = "" }
            }
        };

        var response = await Client.PostAsJsonAsync($"{BasePath}/full", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Take_parametresi_sonuc_sayisini_sinirlar()
    {
        await TestData.UrunOlustur(Client);
        await TestData.UrunOlustur(Client);
        await TestData.UrunOlustur(Client);

        var sonuc = await Client.GetFromJsonAsync<List<TestData.ProductPayload>>($"{BasePath}?take=2");

        Assert.True(sonuc!.Count <= 2, $"take=2 istendi ama {sonuc.Count} kayıt döndü.");
    }

    private sealed record UpdateProductPayload(
        string ProductCode,
        string Name,
        string? ShortDescription,
        int Kind,
        int Status,
        bool IsActive,
        bool IsSellable,
        bool IsPurchasable,
        bool TrackInventory,
        string DefaultCurrencyCode,
        decimal? TaxRate);
}
