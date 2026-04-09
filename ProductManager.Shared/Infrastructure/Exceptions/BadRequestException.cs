namespace ProductManager.Shared.Infrastructure.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message = "Geçersiz istek.")
            : base(message, 400)
        {
        }

        public BadRequestException(string message, Dictionary<string, string[]>? errors)
            : base(message, 400)
        {
            Errors = errors;
        }

        public Dictionary<string, string[]>? Errors { get; }
    }
}
