using ProductManagement.Service.Shared.Abstract;
using Deva.Extensions.G2way.Abstracts;
using Microsoft.Extensions.Logging;

namespace ProductManagement.Service.Concrete
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly Lazy<IDevaGatewayService> _devaGatewayService;

        public EmailService(
            IHttpClientFactory httpClientFactory,
            ILogger<EmailService> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            Lazy<IDevaGatewayService> devaGatewayService)
        {
            _logger = logger;

            var mailjetAdapter = configuration.GetSection("DevaGateway:Adapters")
                .GetChildren()
                .FirstOrDefault(a => a["Adapter"] == "Mailjet");

            _devaGatewayService = devaGatewayService;
        }

        public async Task SendDealerAccountCreatedEmailAsync(string toEmail, string dealerName, string username, string password)
        {
            var subject = "B2B Portal - Hesabınız Oluşturuldu";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hoş Geldiniz!</h2>
                    <p>Sayın <strong>{dealerName}</strong>,</p>
                    <p>B2B Bayi Portalı hesabınız başarıyla oluşturulmuştur.</p>
                    
                    <div style='background-color: #f5f5f5; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h3>Giriş Bilgileriniz:</h3>
                        <p><strong>Kullanıcı Adı (Email):</strong> {username}</p>
                        <p><strong>Geçici Şifre:</strong> {password}</p>
                    </div>
                    
                    <p><strong>⚠️ Güvenlik Uyarısı:</strong></p>
                    <ul>
                        <li>İlk girişinizde lütfen şifrenizi değiştirin</li>
                        <li>Bu şifreyi kimseyle paylaşmayın</li>
                        <li>Bu e-postayı okuduktan sonra silin</li>
                    </ul>
                    
                    <p>Portal adresinize giriş yapmak için:</p>
                    <a href='https://bayiportal.godeva.com.tr' style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>
                        Portala Giriş Yap
                    </a>
                    
                    <hr style='margin: 30px 0;'>
                    <p style='color: #666; font-size: 12px;'>
                        Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.<br>
                        Sorularınız için destek ekibimizle iletişime geçebilirsiniz.
                    </p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                await _devaGatewayService.Value.SendMailAsync(new()
                {
                    To = new()
                    {
                        toEmail
                    },
                    Subject = subject,
                    Html = body,
                    Attachments = null,
                    ReturnException = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gönderilirken hata oluştu: {Email}", toEmail);
            }
        }
    }
}
