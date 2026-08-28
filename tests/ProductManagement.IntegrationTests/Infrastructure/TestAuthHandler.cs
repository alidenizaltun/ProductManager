using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// Veri testlerinde her isteği sabit bir yönetici kullanıcı olarak doğrular.
///
/// PM'de yetkilendirme <c>[RequirePermission]</c> ile claim tabanlı yapıldığı için,
/// bu şema tüm izinleri taşıyan bir claim seti üretir. Amaç izin motorunu test etmek değil,
/// veri davranışını yetkilendirme gürültüsü olmadan sınamaktır. Gerçek sözleşme
/// <see cref="Contracts.AuthorizationContractTests"/> içinde, bu şema devrede
/// <b>olmadan</b> doğrulanır.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId.ToString()),
            new(ClaimTypes.Name, "integration-test"),
            new(ClaimTypes.Email, "test@example.invalid"),
            new(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
