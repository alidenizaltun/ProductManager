using System.Net;
using System.Net.Http.Json;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Endpoints;

/// <summary>
/// Zam yönetimi uç noktalarının davranış sözleşmesi.
///
/// Durum makinesi: Taslak(1) → önizle → Önizlendi(2) → gönder → Onay bekliyor(3) →
/// onayla → Onaylandı(4) → uygula → Uygulandı(5) → geri al → Geri alındı(6).
/// Sıra dışı bir geçiş (ör. önizlemeden onaya göndermek) 409 döner — <c>EnsureRevisionStatusAsync</c>
/// bunu <c>ConflictException</c> ile korur. Gerçek SQL Server gerektirir — Docker kapalıysa atlanır.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class PriceRevisionsEndpointTests
{
    private readonly DatabaseFixture _fixture;
    private HttpClient? _cached;

    public PriceRevisionsEndpointTests(DatabaseFixture fixture) => _fixture = fixture;

    private HttpClient Client => _cached ??= _fixture.Factory.CreateApiClient();

    private const string BasePath = "/api/price-revisions";

    private static object YeniRevizyon() => new
    {
        Name = $"Test Zam {Guid.NewGuid().ToString("N")[..6]}",
        AdjustmentType = 1, // Percent
        Value = 10m,
        RoundingMode = 1, // None
    };

    [DockerFact]
    public async Task Listeleme_200_doner()
    {
        var response = await Client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturma_201_doner_ve_kod_uretir()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniRevizyon());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<RevisionPayload>();
        Assert.StartsWith("ZAM-", created!.Code);
        Assert.Equal(1, created.Status);
    }

    [DockerFact]
    public async Task Bos_ad_400_doner()
    {
        var gecersiz = new { Name = "", AdjustmentType = 1, Value = 10m, RoundingMode = 1 };

        var response = await Client.PostAsJsonAsync(BasePath, gecersiz);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [DockerFact]
    public async Task Olusturulan_kayit_kimligiyle_geri_okunabilir()
    {
        var created = await RevizyonOlusturVeOku();

        var response = await Client.GetAsync($"{BasePath}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task Olmayan_kimlik_404_doner()
    {
        var response = await Client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerFact]
    public async Task Guncelleme_204_doner_ve_degisiklik_kalici_olur()
    {
        var created = await RevizyonOlusturVeOku();
        var guncel = new { Code = created.Code, Name = "Güncellenmiş Zam", AdjustmentType = 1, Value = 15m, RoundingMode = 1 };

        var response = await Client.PutAsJsonAsync($"{BasePath}/{created.Id}", guncel);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RevisionPayload>($"{BasePath}/{created.Id}");
        Assert.Equal("Güncellenmiş Zam", okunan!.Name);
    }

    [DockerFact]
    public async Task Silinen_kayit_artik_bulunamaz()
    {
        var created = await RevizyonOlusturVeOku();

        var silme = await Client.DeleteAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okuma = await Client.GetAsync($"{BasePath}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, okuma.StatusCode);
    }

    // ── Kapsam ───────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Kapsam_olusturma_201_doner()
    {
        var revizyon = await RevizyonOlusturVeOku();
        var urun = await TestData.UrunOlustur(Client);
        var payload = new { ScopeType = 1, TargetId = urun.Id, IsExclude = false }; // ScopeType 1 = Product

        var response = await Client.PostAsJsonAsync($"{BasePath}/{revizyon.Id}/scopes", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [DockerFact]
    public async Task Kapsam_silinen_kayit_artik_kapsam_listesinde_gorunmez()
    {
        var revizyon = await RevizyonOlusturVeOku();
        var urun = await TestData.UrunOlustur(Client);
        var olusturma = await Client.PostAsJsonAsync(
            $"{BasePath}/{revizyon.Id}/scopes", new { ScopeType = 1, TargetId = urun.Id, IsExclude = false });
        var scope = await olusturma.Content.ReadFromJsonAsync<ScopePayload>();

        var silme = await Client.DeleteAsync($"{BasePath}/{revizyon.Id}/scopes/{scope!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, silme.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RevisionPayload>($"{BasePath}/{revizyon.Id}");
        Assert.DoesNotContain(okunan!.Scopes, s => s.Id == scope.Id);
    }

    // ── Durum makinesi: sıra dışı geçiş 409 döner ───────────────────────────

    [DockerFact]
    public async Task Onizlemeden_onaya_gondermek_409_doner()
    {
        var revizyon = await RevizyonOlusturVeOku();

        var response = await Client.PostAsync($"{BasePath}/{revizyon.Id}/submit", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [DockerFact]
    public async Task Onaya_gondermeden_onaylamak_409_doner()
    {
        var urun = await TestData.UrunOlustur(Client);
        var revizyon = await RevizyonOlusturVeOku();
        var kapsam = await Client.PostAsJsonAsync(
            $"{BasePath}/{revizyon.Id}/scopes", new { ScopeType = 1, TargetId = urun.Id, IsExclude = false });
        kapsam.EnsureSuccessStatusCode();

        var onizleme = await Client.PostAsync($"{BasePath}/{revizyon.Id}/preview", null);
        onizleme.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync($"{BasePath}/{revizyon.Id}/approve", new { });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── Mutlu yol: taslak → önizle → gönder → onayla → uygula → geri al ────

    [DockerFact]
    public async Task Tam_akis_taslaktan_geri_almaya_kadar_basarili_olur()
    {
        var urun = await TestData.UrunOlustur(Client);
        await TestData.UrunFiyatiOlustur(Client, urun.Id, tutar: 100m);

        var revizyon = await RevizyonOlusturVeOku();
        var kapsamOlusturma = await Client.PostAsJsonAsync(
            $"{BasePath}/{revizyon.Id}/scopes", new { ScopeType = 1, TargetId = urun.Id, IsExclude = false });
        kapsamOlusturma.EnsureSuccessStatusCode();

        var onizleme = await Client.PostAsync($"{BasePath}/{revizyon.Id}/preview", null);
        Assert.Equal(HttpStatusCode.OK, onizleme.StatusCode);
        var ozet = await onizleme.Content.ReadFromJsonAsync<SummaryPayload>();
        Assert.True(ozet!.LineCount >= 1);

        var gonderme = await Client.PostAsync($"{BasePath}/{revizyon.Id}/submit", null);
        Assert.Equal(HttpStatusCode.NoContent, gonderme.StatusCode);

        var onaylama = await Client.PostAsJsonAsync($"{BasePath}/{revizyon.Id}/approve", new { });
        Assert.Equal(HttpStatusCode.NoContent, onaylama.StatusCode);

        var uygulama = await Client.PostAsync($"{BasePath}/{revizyon.Id}/apply", null);
        Assert.Equal(HttpStatusCode.OK, uygulama.StatusCode);
        var uygulamaSonucu = await uygulama.Content.ReadFromJsonAsync<ExecutionResultPayload>();
        Assert.Equal(5, uygulamaSonucu!.Status); // Applied

        var geriAlma = await Client.PostAsync($"{BasePath}/{revizyon.Id}/rollback", null);
        Assert.Equal(HttpStatusCode.OK, geriAlma.StatusCode);
        var geriAlmaSonucu = await geriAlma.Content.ReadFromJsonAsync<ExecutionResultPayload>();
        Assert.Equal(6, geriAlmaSonucu!.Status); // RolledBack
    }

    [DockerFact]
    public async Task Onay_bekleyen_revizyon_reddedilebilir()
    {
        var urun = await TestData.UrunOlustur(Client);
        await TestData.UrunFiyatiOlustur(Client, urun.Id); // submit satırsız revizyonu 400 ile reddeder
        var revizyon = await RevizyonOlusturVeOku();
        // Kapsam olmadan önizleme paylaşılan test veritabanındaki her ürünü tarar; burada
        // yalnızca kendi ürünümüzle sınırlayarak taramayı hem hızlı hem deterministik tutuyoruz.
        var kapsam = await Client.PostAsJsonAsync(
            $"{BasePath}/{revizyon.Id}/scopes", new { ScopeType = 1, TargetId = urun.Id, IsExclude = false });
        kapsam.EnsureSuccessStatusCode();

        var onizleme = await Client.PostAsync($"{BasePath}/{revizyon.Id}/preview", null);
        onizleme.EnsureSuccessStatusCode();

        var gonderme = await Client.PostAsync($"{BasePath}/{revizyon.Id}/submit", null);
        gonderme.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync($"{BasePath}/{revizyon.Id}/reject", new { Note = "Bütçe onayı yok" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RevisionPayload>($"{BasePath}/{revizyon.Id}");
        Assert.Equal(7, okunan!.Status); // Rejected
    }

    [DockerFact]
    public async Task Taslak_revizyon_iptal_edilebilir()
    {
        var revizyon = await RevizyonOlusturVeOku();

        var response = await Client.PostAsync($"{BasePath}/{revizyon.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var okunan = await Client.GetFromJsonAsync<RevisionPayload>($"{BasePath}/{revizyon.Id}");
        Assert.Equal(8, okunan!.Status); // Cancelled
    }

    private async Task<RevisionPayload> RevizyonOlusturVeOku()
    {
        var response = await Client.PostAsJsonAsync(BasePath, YeniRevizyon());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RevisionPayload>())!;
    }

    private sealed record RevisionPayload(Guid Id, string Code, string Name, int Status, IReadOnlyList<ScopePayload> Scopes);
    private sealed record ScopePayload(Guid Id, int ScopeType, Guid? TargetId);
    private sealed record SummaryPayload(int LineCount, int ExcludedLineCount, int ProductCount);
    private sealed record ExecutionResultPayload(Guid PriceRevisionId, int Status, int AffectedLineCount, int SkippedLineCount);
}
