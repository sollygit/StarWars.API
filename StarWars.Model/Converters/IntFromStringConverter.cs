using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarWars.Model.Converters
{
    public class IntFromStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int number))
                return number;

            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
                return result;

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

}
