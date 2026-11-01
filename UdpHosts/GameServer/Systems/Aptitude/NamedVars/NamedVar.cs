using System;
using System.Collections.Generic;
using Serilog;

namespace GameServer.Systems.Aptitude.NamedVars;

public sealed class NamedVar
{
    private static readonly ILogger _logger = Log.ForContext<NamedVar>();

    private Dictionary<uint, float> _values;

    // packed key -> number of UiNamedVariable commands watching it
    private Dictionary<uint, int> _uiWatches;

    // watched keys whose value hasn't been sent yet
    private HashSet<uint> _uiDirty;

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

    public bool HasUiChanges => _uiDirty?.Count > 0;

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
        var raw = key.ToStoreKey();
        var changed = !_values.TryGetValue(raw, out var previous) || previous != value;
        _values[raw] = value;

        if (changed && _uiWatches?.ContainsKey(raw) == true)
        {
            (_uiDirty ??= []).Add(raw);
        }

        return true;
    }

    public void Watch(NamedVarKey key)
    {
        _uiWatches ??= [];
        var raw = key.ToStoreKey();
        var count = _uiWatches.GetValueOrDefault(raw);
        _uiWatches[raw] = count + 1;

        if (count == 0 && _values?.ContainsKey(raw) == true)
        {
            (_uiDirty ??= []).Add(raw);
        }
    }

    public void Unwatch(NamedVarKey key)
    {
        var raw = key.ToStoreKey();
        if (_uiWatches is null || !_uiWatches.TryGetValue(raw, out var count))
        {
            return;
        }

        if (count > 1)
        {
            _uiWatches[raw] = count - 1;
            return;
        }

        _uiWatches.Remove(raw);
        _uiDirty?.Remove(raw);
    }

    public void FlushUi(Action<NamedVarKey, float> send)
    {
        if (!HasUiChanges)
        {
            return;
        }

        foreach (var raw in _uiDirty)
        {
            send(NamedVarKey.FromStoreKey(raw), _values[raw]);
        }

        _uiDirty.Clear();
    }

    public override string ToString()
    {
        return $"{Owner} named vars ({Count}{(IsReadOnly ? ", read only" : string.Empty)})";
    }
}