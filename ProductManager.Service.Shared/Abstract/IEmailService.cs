namespace ProductManager.Service.Shared.Abstract
{
    public interface IEmailService
    {
        Task SendDealerAccountCreatedEmailAsync(string toEmail, string dealerName, string username, string password);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
