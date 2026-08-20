namespace ProductManagement.Shared.Infrastructure.Exceptions
{
    public class BaseException : Exception
    {
        public virtual object? AdditionalData { get; set; }

        public int StatusCode { get; set; }

        public BaseException(string message = "Beklenmedik bir sistem hatası oluştu.", int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }

        public BaseException(string message = "Beklenmedik bir sistem hatası oluştu.", int statusCode = 500, Exception? innerException = null) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public BaseException(string message = "Beklenmedik bir sistem hatası oluştu.", int statusCode = 500, object? data = null) : base(message)
        {
            StatusCode = statusCode;
            AdditionalData = data;
        }
    }
}
