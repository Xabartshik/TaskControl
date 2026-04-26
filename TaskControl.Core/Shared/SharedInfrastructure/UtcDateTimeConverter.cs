using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskControl.Web.Infrastructure
{
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // При получении дат от клиента (если он присылает их без таймзоны), 
            // сервер будет сразу считать их UTC
            var date = reader.GetDateTime();
            return date.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                : date.ToUniversalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // При отправке клиенту: если БД отдала Unspecified, притворяемся, что это UTC
            var utcDate = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();

            // Формат ISO 8601 с 'Z' на конце
            writer.WriteStringValue(utcDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}