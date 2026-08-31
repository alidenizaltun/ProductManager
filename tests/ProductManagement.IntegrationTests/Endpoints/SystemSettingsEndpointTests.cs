using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Sistem ayarları uç noktalarının davranış sözleşmesi.
///
/// Bu controller'ın ekleme/silme uç noktası yok — ayarlar seed edilir, yalnızca toplu
/// güncellenir. Taze bir test veritabanında hiç ayar kaydı olmadığı için buradaki testler
/// listelemenin çalıştığını ve bilinmeyen bir kimlikle güncellemenin 404 verdiğini doğrular.
/// Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class SystemSettingsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public SystemSettingsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync("/api/system-settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Bos_liste_ile_toplu_guncelleme_204_doner()
    {
        var response = await Client.PutAsJsonAsync("/api/system-settings", new { Items = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_ayar_kimligiyle_guncelleme_404_doner()
    {
        var payload = new { Items = new[] { new { Id = Guid.NewGuid(), Value = "test" } } };

        var response = await Client.PutAsJsonAsync("/api/system-settings", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
