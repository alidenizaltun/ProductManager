using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Herkese açık ürün vitrini uç noktalarının davranış sözleşmesi.
///
/// Anonim erişilebilirlik <c>AuthorizationContractTests</c> tarafından ayrıca doğrulanır;
/// burada yalnızca veri davranışı sınanır. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class PublicProductsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public PublicProductsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/public/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_urun_vitrinde_gorunur()
    {
        var created = await TestData.UrunOlustur(Client);

        var response = await Client.GetAsync($"/api/public/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_urun_404_doner()
    {
        var response = await Client.GetAsync($"/api/public/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
