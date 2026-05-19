#nullable enable
using System.Collections.Generic;

namespace GameServer.Physics.IniLoader;

public class IniData
{
    public Dictionary<string, Dictionary<string, string>> Sections { get; } = [];

    public string? GetValue(string section, string key)
    {
        if (Sections.TryGetValue(section, out var kv) && kv.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }
}
