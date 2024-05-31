using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer.Physics.ZoneLoader;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array.");
        }

        float[] values = new float[3];
        int index = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.Number || index >= 4)
            {
                throw new JsonException("Invalid vector format.");
            }

            values[index++] = reader.GetSingle();
        }

        if (index != 3)
        {
            throw new JsonException("Invalid vector format.");
        }

        return new Vector3(values[0], values[1], values[2]);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}