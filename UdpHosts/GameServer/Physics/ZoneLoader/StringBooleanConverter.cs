using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer.Physics.ZoneLoader;

public class StringBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string boolString = reader.GetString();
            if (bool.TryParse(boolString, out bool result))
            {
                return result;
            }

            throw new JsonException($"Unable to convert \"{boolString}\" to boolean.");
        }

        throw new JsonException($"Unexpected token parsing boolean. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLower());
    }
}