using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer.Physics.TagfileLoader;

public class TagfileObjectDictionaryConverter : JsonConverter<Dictionary<string, BaseTagfileObject>>
{
    public override Dictionary<string, BaseTagfileObject> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new Dictionary<string, BaseTagfileObject>();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        var array = JsonDocument.ParseValue(ref reader).RootElement;

        foreach (var element in array.EnumerateArray())
        {
            var tagfileObject = JsonSerializer.Deserialize<BaseTagfileObject>(element.GetRawText(), options);
            if (tagfileObject == null || string.IsNullOrEmpty(tagfileObject.Name))
            {
                throw new JsonException("Invalid BaseTagfileObject or missing Name");
            }

            result[tagfileObject.Name] = tagfileObject;
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, BaseTagfileObject> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var kvp in value)
        {
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndArray();
    }
}
