using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarWars.Model.Converters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        // Add tis format 2005-10-24T12:10:16.128Z
        private readonly string[] dateFormats = new[] {
            "dd MMM yyyy",   // e.g., "25 May 1977"
            "d MMM yyyy",    // handles single-digit day: "5 May 1977"
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd",
            "MM/dd/yyyy"    // fallback US format
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (DateTime.TryParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Optional: fallback or throw an exception
            throw new JsonException($"Unable to convert '{value}' to DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Write back in a standard ISO format
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }
}
