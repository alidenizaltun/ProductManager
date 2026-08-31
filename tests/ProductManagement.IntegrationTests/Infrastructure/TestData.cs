using System.Net.Http.Json;

namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// Testlerin ihtiyaç duyduğu ön koşul kayıtlarını API üzerinden oluşturur.
///
/// Doğrudan DbContext'e yazmak yerine gerçek uçları kullanır: böylece kurulum adımı da
/// sözleşmeyi doğrular ve testler veritabanı şemasına değil, API'ye bağlı kalır.
/// </summary>
public static class TestData
{
    /// <summary>Testin kendi tanımladığı ürün sözleşmesi (üretim DTO'su kasıtlı olarak kullanılmaz).</summary>
    public sealed record ProductPayload(
        Guid Id,
        string ProductCode,
        string Name,
        int Kind,
        int Status,
        bool IsActive,
        bool IsSellable,
        string DefaultCurrencyCode);

    public sealed record CreateProductPayload(
        string? ProductCode,
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

    public static CreateProductPayload YeniUrun(string? kod = null, string? ad = null) => new(
        ProductCode: kod,
        Name: ad ?? $"Test Ürünü {Guid.NewGuid().ToString("N")[..6]}",
        ShortDescription: "Entegrasyon testi tarafından oluşturuldu",
        Kind: 1,
        Status: 1,
        IsActive: true,
        IsSellable: true,
        IsPurchasable: true,
        TrackInventory: true,
        DefaultCurrencyCode: "TRY",
        TaxRate: 20m);

    /// <summary>Bir ürün oluşturur ve oluşturulan kaydı döndürür.</summary>
    public static async Task<ProductPayload> UrunOlustur(HttpClient client, string? ad = null)
    {
        var response = await client.PostAsJsonAsync("/api/products", YeniUrun(ad: ad));
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ProductPayload>();
        return created!;
    }

    // ── Birim tanımı ──────────────────────────────────────────────────────────

    public sealed record UnitDefinitionPayload(Guid Id, string Code, string Name);

    /// <summary>
    /// Bir birim tanımı oluşturur. Ürün birimleri bu tanıma bağlandığı için,
    /// birim testlerinin ön koşuludur.
    /// </summary>
    public static async Task<UnitDefinitionPayload> BirimTanimiOlustur(HttpClient client)
    {
        var payload = new
        {
            Code = (string?)null,
            Name = $"Adet {Guid.NewGuid().ToString("N")[..6]}",
            Description = "Entegrasyon testi tarafından oluşturuldu",
            IsActive = true,
            SortOrder = 0,
        };

        var response = await client.PostAsJsonAsync("/api/unit-definitions", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<UnitDefinitionPayload>())!;
    }

    // ── Öznitelik tanımı ──────────────────────────────────────────────────────

    public sealed record AttributeDefinitionPayload(Guid Id, string Key, string DisplayName, int DataType);

    public static async Task<AttributeDefinitionPayload> OznitelikTanimiOlustur(HttpClient client)
    {
        var payload = new
        {
            Key = $"test_ozellik_{Guid.NewGuid().ToString("N")[..6]}",
            DisplayName = "Test Özelliği",
            DataType = 1,
            IsRequired = false,
            IsFilterable = false,
            IsVariantAxis = false,
        };

        var response = await client.PostAsJsonAsync("/api/attributes", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AttributeDefinitionPayload>())!;
    }

    // ── Kategori ──────────────────────────────────────────────────────────────

    public sealed record CategoryPayload(Guid Id, string Code, string Name);

    public static async Task<CategoryPayload> KategoriOlustur(HttpClient client)
    {
        var payload = new { Name = $"Test Kategori {Guid.NewGuid().ToString("N")[..6]}" };

        var response = await client.PostAsJsonAsync("/api/catalog/categories", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CategoryPayload>())!;
    }

    // ── Tedarikçi ─────────────────────────────────────────────────────────────

    public sealed record SupplierPayload(Guid Id, string SupplierCode, string Name);

    public static async Task<SupplierPayload> TedarikciOlustur(HttpClient client)
    {
        var payload = new { Name = $"Test Tedarikçi {Guid.NewGuid().ToString("N")[..6]}", IsActive = true };

        var response = await client.PostAsJsonAsync("/api/catalog/suppliers", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<SupplierPayload>())!;
    }

    // ── Ürün fiyatı ───────────────────────────────────────────────────────────

    public sealed record ProductPricePayload(Guid Id, Guid ProductId, int PriceType, decimal Amount, string CurrencyCode);

    public static async Task<ProductPricePayload> UrunFiyatiOlustur(HttpClient client, Guid productId, decimal tutar = 100m)
    {
        var payload = new
        {
            ProductId = productId,
            PriceType = 1,
            Amount = tutar,
            CurrencyCode = "TRY",
        };

        var response = await client.PostAsJsonAsync($"/api/products/{productId}/prices", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProductPricePayload>())!;
    }

    // ── Bölge ─────────────────────────────────────────────────────────────────

    public sealed record RegionPayload(Guid Id, string Code, string Name);

    public static async Task<RegionPayload> BolgeOlustur(HttpClient client)
    {
        var payload = new { Name = $"Test Bölge {Guid.NewGuid().ToString("N")[..6]}" };

        var response = await client.PostAsJsonAsync("/api/regions", payload);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RegionPayload>())!;
    }
}
