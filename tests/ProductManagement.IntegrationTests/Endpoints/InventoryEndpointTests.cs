using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Inventory uç noktalarının (stok kaydı, hareket, rezervasyon) davranış sözleşmesi.
///
/// <c>WarehouseCode</c> denormalize bir alan — gerçek bir depo kaydına FK zorunluluğu yok,
/// bu yüzden testler ayrı bir depo oluşturmadan sabit bir kod kullanır. Her kayıt bir ürüne
/// bağlıdır. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class InventoryEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public InventoryEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    // ── Stok kayıtları ───────────────────────────────────────────────────────

    [DockerFact]
    public async Task Stok_listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/inventory/inventories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Stok_olusturma_201_doner_ve_konum_basligi_verir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { ProductId = urun.Id, WarehouseCode = "WH-TEST", QuantityOnHand = 10m, QuantityReserved = 0m };

        var response = await Client.PostAsJsonAsync("/api/inventory/inventories", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<InventoryPayload>();
        Assert.Equal(10m, created!.QuantityOnHand);
    }

    [DockerFact]
    public async Task Stok_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/inventory/inventories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Stok_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await StokOlusturVeOku();
        var guncel = new { WarehouseCode = "WH-TEST", QuantityOnHand = 25m, QuantityReserved = 5m, InventoryPolicy = 1 };

        var response = await Client.PutAsJsonAsync($"/api/inventory/inventories/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<InventoryPayload>($"/api/inventory/inventories/{created.Id}");
        Assert.Equal(25m, okunan!.QuantityOnHand);
    }

    [DockerFact]
    public async Task Stok_silinen_kayit_artik_bulunamaz()
    {
        var created = await StokOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/inventory/inventories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/inventory/inventories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private async Task<InventoryPayload> StokOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { ProductId = urun.Id, WarehouseCode = "WH-TEST", QuantityOnHand = 10m, QuantityReserved = 0m };

        var response = await Client.PostAsJsonAsync("/api/inventory/inventories", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InventoryPayload>())!;
    }

    // ── Stok hareketleri ─────────────────────────────────────────────────────

    [DockerFact]
    public async Task Hareket_listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/inventory/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Hareket_olusturma_201_doner_ve_geri_okunabilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { ProductId = urun.Id, TransactionType = 1, Quantity = 5m };

        var response = await Client.PostAsJsonAsync("/api/inventory/transactions", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TransactionPayload>();
        var okuma = await Client.GetAsync($"/api/inventory/transactions/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Hareket_olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/inventory/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Rezervasyonlar ───────────────────────────────────────────────────────

    [DockerFact]
    public async Task Rezervasyon_listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/inventory/reservations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Rezervasyon_olusturma_201_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new
        {
            ProductId = urun.Id,
            Quantity = 3m,
            ReservationCode = $"REZ-{Guid.NewGuid().ToString("N")[..8]}",
        };

        var response = await Client.PostAsJsonAsync("/api/inventory/reservations", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [DockerFact]
    public async Task Rezervasyon_durum_guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await RezervasyonOlusturVeOku();
        var guncel = new { Status = 2 };

        var response = await Client.PatchAsJsonAsync($"/api/inventory/reservations/{created.Id}/status", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<ReservationPayload>($"/api/inventory/reservations/{created.Id}");
        Assert.Equal(2, okunan!.Status);
    }

    [DockerFact]
    public async Task Rezervasyon_silinen_kayit_artik_bulunamaz()
    {
        var created = await RezervasyonOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/inventory/reservations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/inventory/reservations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    private async Task<ReservationPayload> RezervasyonOlusturVeOku()
    {
        var urun = await TestData.UrunOlustur(Client);
        var payload = new
        {
            ProductId = urun.Id,
            Quantity = 3m,
            ReservationCode = $"REZ-{Guid.NewGuid().ToString("N")[..8]}",
        };

        var response = await Client.PostAsJsonAsync("/api/inventory/reservations", payload);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReservationPayload>())!;
    }

    private sealed record InventoryPayload(Guid Id, Guid ProductId, decimal QuantityOnHand, decimal QuantityReserved);
    private sealed record TransactionPayload(Guid Id, Guid ProductId, int TransactionType, decimal Quantity);
    private sealed record ReservationPayload(Guid Id, Guid ProductId, decimal Quantity, string ReservationCode, int Status);
}
