using System.Text.Json;

namespace Shared.Content.Json;

internal static class JsonNumbers
{
    public static float[] Read(ref Utf8JsonReader reader, int count, string expected)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected {expected}");
        }

        var values = new float[count];
        for (var i = 0; i < count; i++)
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected {expected}");
            }

            values[i] = reader.GetSingle();
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"Expected {expected}");
        }

        return values;
    }
}
