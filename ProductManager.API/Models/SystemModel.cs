using ProductManager.Shared.Infrastructure.Extensions;

namespace ProductManager.API.Models
{
    public class SystemModel
    {
        public class ErrorDetails
        {
            public string Message { get; set; } = "Beklenmedik bir sistem hatası oluştu.";
            public string ErrorCode { get; set; } = null!;
            public int StatusCode { get; set; } = 500;

            public object? AdditionalData { get; set; }

            public override string ToString()
            {
                return this.ToJson(System.Text.Json.JsonNamingPolicy.CamelCase);
            }
        }
    }
}
