using System.Text.Json.Serialization;

namespace Shared.Content;

/// <summary>
///     One entry of a content file. <see cref="Notes"/> take the place of comments, tools write these files and don't keep
///     comments.
/// </summary>
public abstract record ContentEntry
{
    /// <summary>Unique within its file, entries are sorted by it.</summary>
    [JsonIgnore]
    public abstract string Key { get; }

    /// <summary>Where an imported entry came from, e.g. a replay or capture entity.</summary>
    [JsonPropertyOrder(100)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; init; }

    /// <summary>Set once a person changed an imported entry, a re-import leaves it alone then.</summary>
    [JsonPropertyOrder(101)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Edited { get; init; }

    [JsonPropertyOrder(102)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Notes { get; init; }
}
