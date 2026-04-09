namespace ProductManager.Shared.Infrastructure.Exceptions
{
    public class AuthorizationException : BaseException
    {
        public AuthorizationException(string message = "Bu işlem için yetkiniz bulunmamaktadır.")
            : base(message, 403)
        {
        }

        public AuthorizationException(string message, object? data)
            : base(message, 403, data)
        {
        }
    }
}
