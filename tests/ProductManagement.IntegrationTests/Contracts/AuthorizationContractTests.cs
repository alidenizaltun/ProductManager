using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.IntegrationTests.Infrastructure;

namespace ProductManagement.IntegrationTests.Contracts;

/// <summary>
/// Yetkilendirme sozlesmesi: <b>her uc nokta ya yetki ister ya da bilincli olarak herkese aciktir.</b>
///
/// PM API'sinde su an 149 uc nokta hicbir yetkilendirme tasimiyor (bkz. Faz 1 sozlesme raporu).
/// Bu testi kalici kirmizi birakmak yerine bir <i>circir</i> olarak kuruyoruz:
///
///   - Asagidaki taban liste, 27.08.2026 itibariyla bilinen aciktir. Kayit altindadir.
///   - Listede olmayan yeni bir korumasiz uc nokta eklenirse test <b>aninda kirilir</b>.
///   - Bir uc noktaya yetkilendirme eklendikce ilgili satir buradan silinir.
///   - Liste bosaldiginda test, B2B'deki muadili gibi tam sozlesmeyi zorlar.
///
/// Yani bu dosya hem bir guvenlik borcu defteri hem de borcun buyumesini engelleyen bir kilittir.
/// </summary>
public class AuthorizationContractTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthorizationContractTests(ApiFactory factory) => _factory = factory;

    /// <summary>Kimlik dogrulamasi olmadan cagrilabilmesi kasitli olan yollar.</summary>
    private static readonly HashSet<string> PubliclyReachableByDesign = new(StringComparer.OrdinalIgnoreCase)
    {
        // Oturum acma ve hesap kurtarma akislari
        "POST /api/auth/login",
        "POST /api/auth/register",
        "POST /api/auth/refresh",
        "POST /api/auth/forgot-password",
        "POST /api/auth/reset-password",
        "GET /api/auth/confirm-email",

        // Herkese acik urun vitrini
        "GET /api/public/products",
        "GET /api/public/products/{productId}",
    };

    /// <summary>
    /// 27.08.2026 itibariyla yetkilendirmesi olmayan uc noktalar.
    /// BU LISTEYE SATIR EKLENMEZ — yalnizca yetkilendirme eklendikce satir silinir.
    /// </summary>
    private static readonly HashSet<string> KnownUnprotectedBaseline = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET /api/attributes",
        "POST /api/attributes",
        "DELETE /api/attributes/{attributeDefinitionId}",
        "GET /api/attributes/{attributeDefinitionId}",
        "PUT /api/attributes/{attributeDefinitionId}",
        "GET /api/catalog/categories",
        "POST /api/catalog/categories",
        "DELETE /api/catalog/categories/{categoryId}",
        "GET /api/catalog/categories/{categoryId}",
        "PUT /api/catalog/categories/{categoryId}",
        "GET /api/catalog/suppliers",
        "POST /api/catalog/suppliers",
        "DELETE /api/catalog/suppliers/{supplierId}",
        "GET /api/catalog/suppliers/{supplierId}",
        "PUT /api/catalog/suppliers/{supplierId}",
        "GET /api/catalog/warehouses",
        "POST /api/catalog/warehouses",
        "DELETE /api/catalog/warehouses/{warehouseId}",
        "GET /api/catalog/warehouses/{warehouseId}",
        "PUT /api/catalog/warehouses/{warehouseId}",
        "GET /api/inventory/inventories",
        "POST /api/inventory/inventories",
        "DELETE /api/inventory/inventories/{inventoryId}",
        "GET /api/inventory/inventories/{inventoryId}",
        "PUT /api/inventory/inventories/{inventoryId}",
        "GET /api/inventory/reservations",
        "POST /api/inventory/reservations",
        "DELETE /api/inventory/reservations/{reservationId}",
        "GET /api/inventory/reservations/{reservationId}",
        "PATCH /api/inventory/reservations/{reservationId}/status",
        "GET /api/inventory/transactions",
        "POST /api/inventory/transactions",
        "GET /api/inventory/transactions/{transactionId}",
        "GET /api/lookups",
        "GET /api/lookups/categories",
        "GET /api/lookups/price-lists",
        "GET /api/lookups/products",
        "GET /api/lookups/regions",
        "GET /api/lookups/suppliers",
        "GET /api/lookups/unit-definitions",
        "GET /api/lookups/warehouses",
        "GET /api/pricelists",
        "POST /api/pricelists",
        "DELETE /api/pricelists/{priceListId}",
        "GET /api/pricelists/{priceListId}",
        "PUT /api/pricelists/{priceListId}",
        "GET /api/pricelists/{priceListId}/items",
        "POST /api/pricelists/items",
        "DELETE /api/pricelists/items/{priceListItemId}",
        "GET /api/pricelists/items/{priceListItemId}",
        "PUT /api/pricelists/items/{priceListItemId}",
        "GET /api/products",
        "POST /api/products",
        "DELETE /api/products/{productId}",
        "GET /api/products/{productId}",
        "PUT /api/products/{productId}",
        "GET /api/products/{productId}/attribute-values",
        "POST /api/products/{productId}/attribute-values",
        "GET /api/products/{productId}/bundle-items",
        "POST /api/products/{productId}/bundle-items",
        "GET /api/products/{productId}/category-maps",
        "POST /api/products/{productId}/category-maps",
        "GET /api/products/{productId}/detail",
        "PUT /api/products/{productId}/full",
        "GET /api/products/{productId}/license-offerings",
        "POST /api/products/{productId}/license-offerings",
        "DELETE /api/products/{productId}/license-offerings/{offeringId}",
        "GET /api/products/{productId}/license-offerings/{offeringId}",
        "PUT /api/products/{productId}/license-offerings/{offeringId}",
        "GET /api/products/{productId}/license-offerings/{offeringId}/pricing-parameters",
        "GET /api/products/{productId}/media",
        "POST /api/products/{productId}/media",
        "GET /api/products/{productId}/modules",
        "POST /api/products/{productId}/modules",
        "DELETE /api/products/{productId}/modules/{moduleId}",
        "GET /api/products/{productId}/modules/{moduleId}",
        "PUT /api/products/{productId}/modules/{moduleId}",
        "GET /api/products/{productId}/modules/{moduleId}/offering-prices",
        "POST /api/products/{productId}/modules/{moduleId}/offering-prices",
        "DELETE /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}",
        "GET /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}",
        "PUT /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}",
        "GET /api/products/{productId}/prices",
        "POST /api/products/{productId}/prices",
        "GET /api/products/{productId}/pricing-rules",
        "POST /api/products/{productId}/pricing-rules",
        "PUT /api/products/{productId}/pricing-rules/reorder",
        "DELETE /api/products/{productId}/profiles/physical",
        "GET /api/products/{productId}/profiles/physical",
        "PUT /api/products/{productId}/profiles/physical",
        "DELETE /api/products/{productId}/profiles/service",
        "GET /api/products/{productId}/profiles/service",
        "PUT /api/products/{productId}/profiles/service",
        "DELETE /api/products/{productId}/profiles/software",
        "GET /api/products/{productId}/profiles/software",
        "PUT /api/products/{productId}/profiles/software",
        "DELETE /api/products/{productId}/profiles/subscription",
        "GET /api/products/{productId}/profiles/subscription",
        "PUT /api/products/{productId}/profiles/subscription",
        "GET /api/products/{productId}/regions",
        "POST /api/products/{productId}/regions",
        "GET /api/products/{productId}/supplier-maps",
        "POST /api/products/{productId}/supplier-maps",
        "GET /api/products/{productId}/units",
        "POST /api/products/{productId}/units",
        "GET /api/products/{productId}/variants",
        "POST /api/products/{productId}/variants",
        "DELETE /api/products/attribute-values/{attributeValueId}",
        "GET /api/products/attribute-values/{attributeValueId}",
        "PUT /api/products/attribute-values/{attributeValueId}",
        "DELETE /api/products/bundle-items/{bundleItemId}",
        "GET /api/products/bundle-items/{bundleItemId}",
        "PUT /api/products/bundle-items/{bundleItemId}",
        "DELETE /api/products/category-maps/{categoryMapId}",
        "GET /api/products/category-maps/{categoryMapId}",
        "PUT /api/products/category-maps/{categoryMapId}",
        "POST /api/products/full",
        "DELETE /api/products/media/{mediaId}",
        "GET /api/products/media/{mediaId}",
        "PUT /api/products/media/{mediaId}",
        "DELETE /api/products/prices/{priceId}",
        "GET /api/products/prices/{priceId}",
        "PUT /api/products/prices/{priceId}",
        "DELETE /api/products/pricing-rules/{pricingRuleId}",
        "GET /api/products/pricing-rules/{pricingRuleId}",
        "PUT /api/products/pricing-rules/{pricingRuleId}",
        "POST /api/products/pricing-rules/{pricingRuleId}/save-as-template",
        "DELETE /api/products/regions/{productRegionId}",
        "GET /api/products/regions/{productRegionId}",
        "PUT /api/products/regions/{productRegionId}",
        "DELETE /api/products/supplier-maps/{supplierMapId}",
        "GET /api/products/supplier-maps/{supplierMapId}",
        "PUT /api/products/supplier-maps/{supplierMapId}",
        "DELETE /api/products/units/{productUnitId}",
        "GET /api/products/units/{productUnitId}",
        "PUT /api/products/units/{productUnitId}",
        "DELETE /api/products/variants/{variantId}",
        "GET /api/products/variants/{variantId}",
        "PUT /api/products/variants/{variantId}",
        "GET /api/regions",
        "POST /api/regions",
        "DELETE /api/regions/{id}",
        "GET /api/regions/{id}",
        "PUT /api/regions/{id}",
        "GET /api/unit-definitions",
        "POST /api/unit-definitions",
        "DELETE /api/unit-definitions/{id}",
        "GET /api/unit-definitions/{id}",
        "PUT /api/unit-definitions/{id}",
    };

    [Fact]
    public void Taban_listede_olmayan_yeni_korumasiz_uc_nokta_eklenemez()
    {
        var unprotected = GetApiEndpoints()
            .SelectMany(Describe)
            .Where(x => !PubliclyReachableByDesign.Contains(x))
            .ToList();

        var yeni = unprotected
            .Where(x => !KnownUnprotectedBaseline.Contains(x))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            yeni.Count == 0,
            $"Taban listede olmayan {yeni.Count} korumasiz uc nokta bulundu. Her biri " +
            "[Authorize] ya da [RequirePermission] almali:" +
            Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", yeni));
    }

    [Fact]
    public void Guvenlik_borcu_buyumez()
    {
        var unprotected = GetApiEndpoints()
            .SelectMany(Describe)
            .Where(x => !PubliclyReachableByDesign.Contains(x))
            .Count();

        Assert.True(
            unprotected <= KnownUnprotectedBaseline.Count,
            $"Korumasiz uc nokta sayisi arttI: {unprotected} > {KnownUnprotectedBaseline.Count}.");
    }

    [Fact]
    public async Task Korumali_bir_uc_nokta_tokensiz_istekte_401_doner()
    {
        // RequirePermission gercekten AuthorizeAttribute gibi davraniyor mu?
        var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -- yardimcilar ---------------------------------------------------------

    private IEnumerable<RouteEndpoint> GetApiEndpoints()
    {
        var source = _factory.Services.GetRequiredService<EndpointDataSource>();

        return source.Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText is not null)
            .Where(e => Normalize(e.RoutePattern.RawText).StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .Where(e => !RequiresAuthorization(e));
    }

    private static bool RequiresAuthorization(Endpoint endpoint)
    {
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null) return false;
        return endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null;
    }

    /// <summary>Rota kisitlarini atar: <c>{id:guid}</c> -> <c>{id}</c>.</summary>
    private static string Normalize(string? rawText)
    {
        var path = "/" + (rawText ?? string.Empty).TrimStart('/');
        return System.Text.RegularExpressions.Regex.Replace(path, @"\{(\w+)(:[^}]+)?\??\}", "{$1}");
    }

    private static IEnumerable<string> Describe(RouteEndpoint endpoint)
    {
        var path = Normalize(endpoint.RoutePattern.RawText);
        var verbs = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();

        if (verbs is null || verbs.HttpMethods.Count == 0)
        {
            yield return $"? {path}";
            yield break;
        }

        foreach (var verb in verbs.HttpMethods)
            yield return $"{verb} {path}";
    }
}
