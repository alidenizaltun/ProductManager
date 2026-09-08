using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Ürüne bağlı ilişki uç noktalarının (öznitelik değeri, kategori eşlemesi, medya,
/// bundle kalemi, tedarikçi eşlemesi, bölge) davranış sözleşmesi.
///
/// Her grup kendi FK'sını (öznitelik tanımı, kategori, tedarikçi, bölge, ikinci ürün)
/// önce oluşturur. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ProductRelationsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public ProductRelationsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    // ── Öznitelik değerleri ──────────────────────────────────────────────────

    [DockerFact]
    public async Task Oznitelik_degeri_olusturma_201_doner_ve_listede_gorunur()
    {
        var urun = await TestData.UrunOlustur(Client);
        var tanim = await TestData.OznitelikTanimiOlustur(Client);
        var payload = new { AttributeDefinitionId = tanim.Id, ValueText = "kirmizi" };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/attribute-values", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var liste = await Client.GetFromJsonAsync<List<AttributeValuePayload>>($"/api/products/{urun.Id}/attribute-values");
        Assert.Contains(liste!, v => v.AttributeDefinitionId == tanim.Id);
    }

    [DockerFact]
    public async Task Oznitelik_degeri_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await OznitelikDegeriOlusturVeOku();

        var response = await Client.PutAsJsonAsync($"/api/products/attribute-values/{created.Id}", new { ValueText = "mavi" });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<AttributeValuePayload>($"/api/products/attribute-values/{created.Id}");
        Assert.Equal("mavi", okunan!.ValueText);
    }

    [DockerFact]
    public async Task Oznitelik_degeri_silinen_kayit_artik_bulunamaz()
    {
        var created = await OznitelikDegeriOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/products/attribute-values/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/attribute-values/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Oznitelik_degeri_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/products/attribute-values/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<AttributeValuePayload> OznitelikDegeriOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var tanim = await TestData.OznitelikTanimiOlustur(Client);
        var response = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/attribute-values", new { AttributeDefinitionId = tanim.Id, ValueText = "kirmizi" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttributeValuePayload>())!;
    }

    // ── Kategori eşlemeleri ──────────────────────────────────────────────────

    [DockerFact]
    public async Task Kategori_eslemesi_olusturma_201_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        var kategori = await TestData.KategoriOlustur(Client);
        var payload = new { ProductCategoryId = kategori.Id, IsPrimary = true, SortOrder = 0 };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/category-maps", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_eslemesi_guncelleme_204_doner()
    {
        var created = await KategoriEslemesiOlusturVeOku();

        var response = await Client.PutAsJsonAsync(
            $"/api/products/category-maps/{created.Id}", new { IsPrimary = false, SortOrder = 5 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_eslemesi_silinen_kayit_artik_bulunamaz()
    {
        var created = await KategoriEslemesiOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/products/category-maps/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/category-maps/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private async Task<CategoryMapPayload> KategoriEslemesiOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var kategori = await TestData.KategoriOlustur(Client);
        var response = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/category-maps", new { ProductCategoryId = kategori.Id, IsPrimary = true, SortOrder = 0 });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryMapPayload>())!;
    }

    // ── Medya ────────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Medya_olusturma_201_doner_ve_listede_gorunur()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { Url = "https://example.invalid/gorsel.png" };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/media", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var liste = await Client.GetFromJsonAsync<List<MediaPayload>>($"/api/products/{urun.Id}/media");
        Assert.Single(liste!);
    }

    [DockerFact]
    public async Task Medya_silinen_kayit_artik_bulunamaz()
    {
        var urun = await TestData.UrunOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/media", new { Url = "https://example.invalid/gorsel.png" });
        var created = await olusturma.Content.ReadFromJsonAsync<MediaPayload>();

        var silme = await Client.DeleteAsync($"/api/products/media/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/media/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Medya_dosya_yukleme_201_doner_ve_sunucuda_tutulur()
    {
        var urun = await TestData.UrunOlustur(Client);
        using var content = new MultipartFormDataContent();
        content.Add(CreatePngContent(), "files", "kapak.png");
        content.Add(CreatePngContent(), "files", "galeri.png");

        var response = await Client.PostAsync($"/api/products/{urun.Id}/media/upload", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<List<MediaPayload>>();
        Assert.Equal(2, created!.Count);
        Assert.All(created, item => Assert.StartsWith("/uploads/products/", item.Url));
        Assert.True(created[0].IsPrimary);
        Assert.False(created[1].IsPrimary);

        var fileResponse = await Client.GetAsync(created[0].Url);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
        Assert.Equal("image/png", fileResponse.Content.Headers.ContentType?.MediaType);
    }

    [DockerFact]
    public async Task Medya_dosya_yukleme_desteklenmeyen_turu_400_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent("not-an-image"u8.ToArray())
        {
            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain") }
        }, "files", "notlar.txt");

        var response = await Client.PostAsync($"/api/products/{urun.Id}/media/upload", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Bundle kalemleri ─────────────────────────────────────────────────────

    [DockerFact]
    public async Task Bundle_kalemi_olusturma_201_doner()
    {
        var ana = await TestData.UrunOlustur(Client);
        var alt = await TestData.UrunOlustur(Client);
        var payload = new { BundleProductId = ana.Id, ChildProductId = alt.Id, Quantity = 2m, IsOptional = false };

        var response = await Client.PostAsJsonAsync($"/api/products/{ana.Id}/bundle-items", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<BundleItemPayload>();
        Assert.Equal(alt.Id, created!.ChildProductId);
    }

    [DockerFact]
    public async Task Bundle_kalemi_silinen_kayit_artik_bulunamaz()
    {
        var ana = await TestData.UrunOlustur(Client);
        var alt = await TestData.UrunOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{ana.Id}/bundle-items",
            new { BundleProductId = ana.Id, ChildProductId = alt.Id, Quantity = 1m, IsOptional = false });
        var created = await olusturma.Content.ReadFromJsonAsync<BundleItemPayload>();

        var silme = await Client.DeleteAsync($"/api/products/bundle-items/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);
    }

    // ── Tedarikçi eşlemeleri ─────────────────────────────────────────────────

    [DockerFact]
    public async Task Tedarikci_eslemesi_olusturma_201_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        var tedarikci = await TestData.TedarikciOlustur(Client);
        var payload = new { ProductId = urun.Id, ProductSupplierId = tedarikci.Id, IsPreferred = true };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/supplier-maps", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [DockerFact]
    public async Task Tedarikci_eslemesi_silinen_kayit_artik_bulunamaz()
    {
        var urun = await TestData.UrunOlustur(Client);
        var tedarikci = await TestData.TedarikciOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/supplier-maps",
            new { ProductId = urun.Id, ProductSupplierId = tedarikci.Id, IsPreferred = true });
        var created = await olusturma.Content.ReadFromJsonAsync<SupplierMapPayload>();

        var silme = await Client.DeleteAsync($"/api/products/supplier-maps/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);
    }

    // ── Bölgeler ─────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Urun_bolgesi_olusturma_201_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        var bolge = await TestData.BolgeOlustur(Client);
        var payload = new { ProductId = urun.Id, RegionId = bolge.Id, CurrencyCode = "TRY", IsDefault = true, IsActive = true };

        var response = await Client.PostAsJsonAsync($"/api/products/{urun.Id}/regions", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [DockerFact]
    public async Task Urun_bolgesi_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var urun = await TestData.UrunOlustur(Client);
        var bolge = await TestData.BolgeOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/regions",
            new { ProductId = urun.Id, RegionId = bolge.Id, CurrencyCode = "TRY", IsDefault = true, IsActive = true });
        var created = await olusturma.Content.ReadFromJsonAsync<ProductRegionPayload>();

        var response = await Client.PutAsJsonAsync(
            $"/api/products/regions/{created!.Id}",
            new { RegionId = bolge.Id, CurrencyCode = "USD", IsDefault = true, IsActive = true });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<ProductRegionPayload>($"/api/products/regions/{created.Id}");
        Assert.Equal("USD", okunan!.CurrencyCode);
    }

    [DockerFact]
    public async Task Urun_bolgesi_silinen_kayit_artik_bulunamaz()
    {
        var urun = await TestData.UrunOlustur(Client);
        var bolge = await TestData.BolgeOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"/api/products/{urun.Id}/regions",
            new { ProductId = urun.Id, RegionId = bolge.Id, CurrencyCode = "TRY", IsDefault = true, IsActive = true });
        var created = await olusturma.Content.ReadFromJsonAsync<ProductRegionPayload>();

        var silme = await Client.DeleteAsync($"/api/products/regions/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/products/regions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private static ByteArrayContent CreatePngContent()
    {
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=");
        var part = new ByteArrayContent(png);
        part.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        return part;
    }

    private sealed record AttributeValuePayload(Guid Id, Guid ProductId, Guid AttributeDefinitionId, string? ValueText);
    private sealed record CategoryMapPayload(Guid Id, Guid ProductId, Guid ProductCategoryId, bool IsPrimary);
    private sealed record MediaPayload(Guid Id, Guid ProductId, string Url, bool IsPrimary);
    private sealed record BundleItemPayload(Guid Id, Guid BundleProductId, Guid ChildProductId, decimal Quantity);
    private sealed record SupplierMapPayload(Guid Id, Guid ProductId, Guid ProductSupplierId, bool IsPreferred);
    private sealed record ProductRegionPayload(Guid Id, Guid ProductId, Guid RegionId, string CurrencyCode);
}
