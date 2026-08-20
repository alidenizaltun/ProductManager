namespace ProductManagement.Shared.Infrastructure.Exceptions
{
    public class ValidationException : BaseException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(string message = "Doğrulama hatası oluştu.")
            : base(message, 400)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Bir veya daha fazla doğrulama hatası oluştu.", 400)
        {
            Errors = errors;
            AdditionalData = errors;
        }

        public ValidationException(string field, string error)
            : base("Doğrulama hatası oluştu.", 400)
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
            AdditionalData = Errors;
        }
    }
}
