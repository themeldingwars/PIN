#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Systems.Ai.Behavior;

/// <summary>
///     Parses the raw SDB behavior string <c>Name(key=value,key2=value2,...)</c> into a <see cref="ParsedBehavior" />.
///     Handles empty strings, the literal <c>Null</c>, missing params, quoted string values (which may contain commas),
///     and inconsistent whitespace around <c>=</c> / <c>,</c>.
/// </summary>
public static class BehaviorParser
{
    public static ParsedBehavior Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return ParsedBehavior.Empty;
        }

        raw = raw.Trim();
        if (raw.Equals("Null", StringComparison.OrdinalIgnoreCase))
        {
            return ParsedBehavior.Empty;
        }

        int open = raw.IndexOf('(');
        string name;
        string? inner;
        if (open < 0)
        {
            name = raw.Trim();
            inner = null;
        }
        else
        {
            name = raw[..open].Trim();
            int close = raw.LastIndexOf(')');
            inner = close > open ? raw[(open + 1)..close] : null;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return ParsedBehavior.Empty;
        }

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(inner))
        {
            foreach (var part in SplitTopLevel(inner))
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                int eq = part.IndexOf('=');
                if (eq < 0)
                {
                    dict[part.Trim()] = string.Empty;
                }
                else
                {
                    var key = part[..eq].Trim();
                    var value = part[(eq + 1)..].Trim();
                    if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                    {
                        value = value[1..^1];
                    }

                    if (key.Length > 0)
                    {
                        dict[key] = value;
                    }
                }
            }
        }

        return new ParsedBehavior(name, dict);
    }

    private static IEnumerable<string> SplitTopLevel(string s)
    {
        var parts = new List<string>();
        var builder = new StringBuilder();
        bool inQuotes = false;

        foreach (var c in s)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }

            if (c == ',' && !inQuotes)
            {
                parts.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        if (builder.Length > 0)
        {
            parts.Add(builder.ToString());
        }

        return parts;
    }
}
