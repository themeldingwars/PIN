using System.Collections.Concurrent;

namespace GameServer.Extensions;

/// <summary>
/// Helper for diagnostics that are worth reporting once per id instead of once per command execution.
///
/// Aptitude commands run from several places that are not one-shot: an effect's duration chain is re-evaluated
/// on every update tick for every entity that carries the effect, and client proximity abilities are re-triggered
/// by the client while the player stays in range. A warning from one of those paths repeats dozens of times per
/// second per entity, which buries everything else in the log and costs real time in the console sink while the
/// shard is trying to keep the zone's network traffic moving.
/// </summary>
public static class OnceLog
{
    private static readonly ConcurrentDictionary<string, byte> _seen = new();

    /// <summary>
    /// Returns true the first time <paramref name="key"/> is seen and false for every repeat.
    /// Use the command name plus the sdb id, e.g. <c>OnceLog.ShouldLog((nameof(MyCommand), Params.Id))</c>.
    /// </summary>
    public static bool ShouldLog(object key)
    {
        return _seen.TryAdd(key.ToString(), 0);
    }

    /// <summary>
    /// Forget everything reported so far. Intended for tests.
    /// </summary>
    public static void Reset()
    {
        _seen.Clear();
    }
}
