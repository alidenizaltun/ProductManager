using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductManager.Shared.Infrastructures.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dt = reader.GetDateTime();
            return DateTime.SpecifyKind(dt, DateTimeKind.Local);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
            writer.WriteStringValue(local.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
}
