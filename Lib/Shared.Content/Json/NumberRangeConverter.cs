using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Content.Json;

internal sealed class NumberRangeConverter : JsonConverter<NumberRange>
{
    public override NumberRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetUInt32();
            return new NumberRange(value, value);
        }

        var values = JsonNumbers.Read(ref reader, 2, "a number or [min, max]");
        if (values[0] < 0 || values[1] < 0 || values[0] % 1 != 0 || values[1] % 1 != 0)
        {
            throw new JsonException("Expected whole numbers of 0 or more for [min, max]");
        }

        return new NumberRange((uint)values[0], (uint)values[1]);
    }

    public override void Write(Utf8JsonWriter writer, NumberRange value, JsonSerializerOptions options)
    {
        if (value.Min == value.Max)
        {
            writer.WriteNumberValue(value.Min);
            return;
        }

        writer.WriteStartArray();
        writer.WriteNumberValue(value.Min);
        writer.WriteNumberValue(value.Max);
        writer.WriteEndArray();
    }
}
