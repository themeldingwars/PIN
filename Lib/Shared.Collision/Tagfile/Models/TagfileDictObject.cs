using System.Text.Json.Serialization;

namespace Shared.Collision.Tagfile.Models;

public record TagfileDictObject
{
    public TagfileDictObject(string name, string @class, Dictionary<string, object?> data)
    {
        Name = name;
        Class = @class;
        Data = data;
    }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("class")]
    public string Class { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object?> Data { get; init; } = [];
}
