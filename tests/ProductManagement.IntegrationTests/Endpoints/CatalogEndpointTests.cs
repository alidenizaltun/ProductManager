using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Catalog uç noktalarının (kategori, tedarikçi, depo) davranış sözleşmesi.
///
/// Üç kaynak da kod üretimi paylaşır — <c>Code</c> boş bırakılırsa sistem üretir
/// (CAT-/SUP-/WH-000001). Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class CatalogEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public CatalogEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    // ── Kategoriler ──────────────────────────────────────────────────────────

    private const string CategoriesPath = "/api/catalog/categories";

    private static object YeniKategori() => new
    {
        Name = $"Test Kategori {Guid.NewGuid().ToString("N")[..6]}",
        Description = "Entegrasyon testi tarafından oluşturuldu",
    };

    [DockerFact]
    public async Task Kategori_listeleme_200_doner()
    {
        var response = await Client.GetAsync(CategoriesPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_olusturma_201_doner_ve_kod_uretir()
    {
        var response = await Client.PostAsJsonAsync(CategoriesPath, YeniKategori());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<CategoryPayload>();
        Assert.NotNull(created);
        Assert.StartsWith("CAT-", created!.Code);
    }

    [DockerFact]
    public async Task Kategori_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{CategoriesPath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await KategoriOlusturVeOku();
        var guncel = new { Code = created.Code, Name = "Güncellenmiş Kategori", Description = (string?)null, ParentCategoryId = (Guid?)null };

        var response = await Client.PutAsJsonAsync($"{CategoriesPath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<CategoryPayload>($"{CategoriesPath}/{created.Id}");
        Assert.Equal("Güncellenmiş Kategori", okunan!.Name);
    }

    [DockerFact]
    public async Task Kategori_olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new { Code = "CAT-000000", Name = "Ad", Description = (string?)null, ParentCategoryId = (Guid?)null };

        var response = await Client.PutAsJsonAsync($"{CategoriesPath}/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_silinen_kayit_artik_bulunamaz()
    {
        var created = await KategoriOlusturVeOku();

        var silme = await Client.DeleteAsync($"{CategoriesPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{CategoriesPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Kategori_olmayan_kaydi_silmek_404_doner()
    {
        var response = await Client.DeleteAsync($"{CategoriesPath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<CategoryPayload> KategoriOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(CategoriesPath, YeniKategori());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryPayload>())!;
    }

    // ── Tedarikçiler ─────────────────────────────────────────────────────────

    private const string SuppliersPath = "/api/catalog/suppliers";

    private static object YeniTedarikci() => new
    {
        Name = $"Test Tedarikçi {Guid.NewGuid().ToString("N")[..6]}",
        IsActive = true,
    };

    [DockerFact]
    public async Task Tedarikci_listeleme_200_doner()
    {
        var response = await Client.GetAsync(SuppliersPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Tedarikci_olusturma_201_doner_ve_kod_uretir()
    {
        var response = await Client.PostAsJsonAsync(SuppliersPath, YeniTedarikci());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SupplierPayload>();
        Assert.NotNull(created);
        Assert.StartsWith("SUP-", created!.SupplierCode);
    }

    [DockerFact]
    public async Task Tedarikci_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{SuppliersPath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Tedarikci_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await TedarikciOlusturVeOku();
        var guncel = new { SupplierCode = created.SupplierCode, Name = "Güncellenmiş Tedarikçi", IsActive = false };

        var response = await Client.PutAsJsonAsync($"{SuppliersPath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<SupplierPayload>($"{SuppliersPath}/{created.Id}");
        Assert.Equal("Güncellenmiş Tedarikçi", okunan!.Name);
        Assert.False(okunan.IsActive);
    }

    [DockerFact]
    public async Task Tedarikci_silinen_kayit_artik_bulunamaz()
    {
        var created = await TedarikciOlusturVeOku();

        var silme = await Client.DeleteAsync($"{SuppliersPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{SuppliersPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private async Task<SupplierPayload> TedarikciOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(SuppliersPath, YeniTedarikci());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierPayload>())!;
    }

    // ── Depolar ──────────────────────────────────────────────────────────────

    private const string WarehousesPath = "/api/catalog/warehouses";

    private static object YeniDepo() => new
    {
        Name = $"Test Depo {Guid.NewGuid().ToString("N")[..6]}",
        IsActive = true,
    };

    [DockerFact]
    public async Task Depo_listeleme_200_doner()
    {
        var response = await Client.GetAsync(WarehousesPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Depo_olusturma_201_doner_ve_kod_uretir()
    {
        var response = await Client.PostAsJsonAsync(WarehousesPath, YeniDepo());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<WarehousePayload>();
        Assert.NotNull(created);
        Assert.StartsWith("WH-", created!.Code);
    }

    [DockerFact]
    public async Task Depo_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{WarehousesPath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Depo_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await DepoOlusturVeOku();
        var guncel = new { Code = created.Code, Name = "Güncellenmiş Depo", IsActive = false };

        var response = await Client.PutAsJsonAsync($"{WarehousesPath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<WarehousePayload>($"{WarehousesPath}/{created.Id}");
        Assert.Equal("Güncellenmiş Depo", okunan!.Name);
    }

    [DockerFact]
    public async Task Depo_silinen_kayit_artik_bulunamaz()
    {
        var created = await DepoOlusturVeOku();

        var silme = await Client.DeleteAsync($"{WarehousesPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{WarehousesPath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private async Task<WarehousePayload> DepoOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(WarehousesPath, YeniDepo());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WarehousePayload>())!;
    }

    private sealed record CategoryPayload(Guid Id, string Code, string Name, string? Description, Guid? ParentCategoryId);
    private sealed record SupplierPayload(Guid Id, string SupplierCode, string Name, bool IsActive);
    private sealed record WarehousePayload(Guid Id, string Code, string Name, bool IsActive);
}
