using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Content.Json;

internal sealed class QuaternionConverter : JsonConverter<Quaternion>
{
    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var values = JsonNumbers.Read(ref reader, 4, "[x, y, z, w]");
        return new Quaternion(values[0], values[1], values[2], values[3]);
    }

    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteNumberValue(value.W);
        writer.WriteEndArray();
    }
}
