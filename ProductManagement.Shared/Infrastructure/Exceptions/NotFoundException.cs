namespace ProductManagement.Shared.Infrastructure.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message = "İstenen kayıt bulunamadı.")
            : base(message, 404)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} bulunamadı. Key: {key}", 404)
        {
        }
    }
}
