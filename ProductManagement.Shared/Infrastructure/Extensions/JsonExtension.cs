using System.Text.Json;

namespace ProductManagement.Shared.Infrastructure.Extensions
{
    public static class JsonExtension
    {
        public static string ToJson(this object obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, options: new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ToJson(this object obj, JsonNamingPolicy propertyNamingPolicy)
        {
            try
            {
                if (obj == null) return string.Empty;

                return JsonSerializer.Serialize(obj, options: new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = propertyNamingPolicy
                });
            }
            catch
            {
                return string.Empty;
            }
        }

        public static T? ToObject<T>(this string jsonString)
        {
            try
            {
                if (!IsValidJson(jsonString)) return Enumerable.Empty<T>().FirstOrDefault()!;

                return JsonSerializer.Deserialize<T>(jsonString, options: new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                return Enumerable.Empty<T>().FirstOrDefault();
            }
        }

        public static bool IsValidJson(this string? jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString?.Trim()))
                    return false;

                JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch { return false; }
        }

        public static JsonDocument? ToJsonDocument<T>(this T model) where T : class
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(model, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                });

                return JsonDocument.Parse(jsonString);
            }
            catch { return null; }
        }
    }
}
