using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Rol yönetimi uç noktalarının davranış sözleşmesi. Gerçek SQL Server gerektirir —
/// Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RolesEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public RolesEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private static object YeniRol() => new
    {
        Name = $"TestRol_{Guid.NewGuid().ToString("N")[..8]}",
        Description = "Entegrasyon testi tarafından oluşturuldu",
        Permissions = new[] { "Catalog.View" },
    };

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Izin_katalogu_200_doner()
    {
        var response = await Client.GetAsync("/api/roles/permissions/catalog");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_konum_basligi_verir()
    {
        var response = await Client.PostAsJsonAsync("/api/roles", YeniRol());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<RolePayload>();
        Assert.Contains("Catalog.View", created!.Permissions);
    }

    [DockerFact]
    public async Task Bos_ad_400_doner()
    {
        var gecersiz = new { Name = "", Description = (string?)null, Permissions = Array.Empty<string>() };

        var response = await Client.PostAsJsonAsync("/api/roles", gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await RolOlusturVeOku();

        var response = await Client.GetAsync($"/api/roles/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var okunan = await response.Content.ReadFromJsonAsync<RolePayload>();
        Assert.Equal(created.Id, okunan!.Id);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"/api/roles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await RolOlusturVeOku();
        var guncel = new { Description = "Güncellenmiş açıklama", IsActive = true, Permissions = new[] { "Catalog.View", "Catalog.Manage" } };

        var response = await Client.PutAsJsonAsync($"/api/roles/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RolePayload>($"/api/roles/{created.Id}");
        Assert.Equal("Güncellenmiş açıklama", okunan!.Description);
        Assert.Contains("Catalog.Manage", okunan.Permissions);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_guncellemek_404_doner()
    {
        var guncel = new { Description = (string?)null, IsActive = true, Permissions = Array.Empty<string>() };

        var response = await Client.PutAsJsonAsync($"/api/roles/{Guid.NewGuid()}", guncel);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Silinen_kayit_artik_bulunamaz()
    {
        var created = await RolOlusturVeOku();

        var silme = await Client.DeleteAsync($"/api/roles/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"/api/roles/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kaydi_silmek_404_doner()
    {
        var response = await Client.DeleteAsync($"/api/roles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<RolePayload> RolOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync("/api/roles", YeniRol());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RolePayload>())!;
    }

    private sealed record RolePayload(Guid Id, string Name, string? Description, bool IsActive, int UserCount, IReadOnlyList<string> Permissions);
}
