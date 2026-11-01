using System.Collections.Generic;
using Serilog;

namespace GameServer.Systems.Aptitude.NamedVars;

public sealed class NamedVar
{
    private static readonly ILogger _logger = Log.ForContext<NamedVar>();

    private Dictionary<uint, float> _values;

    public NamedVar(string owner)
    {
        Owner = owner;
    }

    private NamedVar(string owner, Dictionary<uint, float> values)
    {
        Owner = owner;
        _values = values;
        IsReadOnly = true;
    }

    public string Owner { get; }

    public bool IsReadOnly { get; }

    public int Count => _values?.Count ?? 0;

    public static NamedVar ReadOnly(string owner, Dictionary<uint, float> values)
    {
        return new NamedVar(owner, values);
    }

    public bool TryGet(NamedVarKey key, out float value)
    {
        if (_values is null)
        {
            value = 0f;
            return false;
        }

        return _values.TryGetValue(key.ToStoreKey(), out value);
    }

    public float Get(NamedVarKey key, float fallback)
    {
        return TryGet(key, out var value) ? value : fallback;
    }

    public bool Set(NamedVarKey key, float value)
    {
        if (IsReadOnly)
        {
            _logger.Warning("{Owner} is read only, dropping named var {Key} = {Value}", Owner, key, value);
            return false;
        }

        _values ??= [];
        _values[key.ToStoreKey()] = value;
        return true;
    }

    public override string ToString()
    {
        return $"{Owner} named vars ({Count}{(IsReadOnly ? ", read only" : string.Empty)})";
    }
}
