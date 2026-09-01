using ProductManagement.Service.Shared.Abstract;

namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// Testlerde gerçek e-posta ağ geçidine (DevaGateway/Mailjet) hiç istek atmaz.
///
/// <c>appsettings.json</c>'daki DevaGateway kimlik bilgileri Testing ortamı için geçersiz
/// kılınmamış; bu yüzden gerçek servis kullanılırsa her davet/kayıt testi gerçek sağlayıcı
/// kotasını tüketir. Bu sahte servis o riski tamamen ortadan kaldırır.
/// </summary>
public sealed class NoOpEmailService : IEmailService
{
    public Task SendDealerAccountCreatedEmailAsync(string toEmail, string dealerName, string username, string password)
        => Task.CompletedTask;

    public Task SendEmailAsync(string toEmail, string subject, string body)
        => Task.CompletedTask;
}
