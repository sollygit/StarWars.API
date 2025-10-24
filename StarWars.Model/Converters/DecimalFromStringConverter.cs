using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarWars.Model.Converters
{
    public class DecimalFromStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If the token is already a number
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out decimal number))
            {
                return number;
            }

            // If the token is a string, we try to parse it
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();

                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }

                return 0m; // Default value if parsing fails
            }

            throw new JsonException($"Unexpected token parsing decimal. TokenType: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
