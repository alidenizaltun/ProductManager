using System.Net;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Lookups uç noktalarının davranış sözleşmesi.
///
/// Tümü salt okunur referans listeleri döndürür — kaynak oluşturma/silme yok, dolayısıyla
/// tek başına anlamlı olan kontrol her uç noktanın 200 döndüğüdür (yetkilendirilmiş bir
/// istemci için). Yetkisiz erişim <c>AuthorizationContractTests</c> tarafından ayrıca kapsanır.
/// Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class LookupsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public LookupsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    [DockerFact]
    public async Task Referans_lookuplari_200_doner()
    {
        var response = await Client.GetAsync("/api/lookups");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerTheory]
    [InlineData("/api/lookups/products")]
    [InlineData("/api/lookups/categories")]
    [InlineData("/api/lookups/warehouses")]
    [InlineData("/api/lookups/suppliers")]
    [InlineData("/api/lookups/price-lists")]
    [InlineData("/api/lookups/unit-definitions")]
    [InlineData("/api/lookups/regions")]
    public async Task Alt_liste_200_doner(string path)
    {
        var response = await Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
