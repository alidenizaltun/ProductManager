namespace ProductManagement.Shared.Infrastructure.Exceptions
{
    public class AuthenticationException : BaseException
    {
        public AuthenticationException(string message = "Kimlik doğrulama başarısız.")
            : base(message, 401)
        {
        }

        public AuthenticationException(string message, object? data)
            : base(message, 401, data)
        {
        }
    }
}
