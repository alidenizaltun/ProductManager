namespace ProductManager.Shared.Infrastructure.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message = "Kayıt zaten mevcut.")
            : base(message, 409)
        {
        }
    }
}
