#nullable enable
using System.Collections.Generic;

namespace GameServer.Systems.Ai.Behavior;

/// <summary>
///     A parsed monster behavior definition. The raw SDB format is <c>Name(key=value,key2=value2,...)</c>.
///     Params are kept as raw strings; consumers parse them on demand.
/// </summary>
public sealed class ParsedBehavior
{
    public ParsedBehavior(string name, Dictionary<string, string>? parameters)
    {
        Name = name;
        Params = parameters ?? new Dictionary<string, string>();
    }

    /// <summary>Empty behavior (no name, no params) for missing/empty/Null source strings.</summary>
    public static ParsedBehavior Empty { get; } = new(string.Empty, null);

    public string Name { get; }
    public Dictionary<string, string> Params { get; }

    public bool IsEmpty => string.IsNullOrEmpty(Name) && Params.Count == 0;

    public bool TryGetParam(string key, out string? value) => Params.TryGetValue(key, out value);
}
