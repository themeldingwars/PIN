using System.Collections.Generic;
using System.Linq;

namespace Shared.Content;

/// <summary>What changed between two versions of a content file, matched by key.</summary>
public sealed record ContentChanges<T>(IReadOnlyList<T> Added, IReadOnlyList<T> Removed, IReadOnlyList<(T Before, T After)> Changed)
    where T : ContentEntry
{
    public bool IsEmpty => Added.Count == 0 && Removed.Count == 0 && Changed.Count == 0;

    public static ContentChanges<T> Between(IReadOnlyList<T> before, IReadOnlyList<T> after)
    {
        var old = before.ToDictionary(e => e.Key);
        var current = after.ToDictionary(e => e.Key);

        // Compared as written, records with lists don't compare by value
        return new ContentChanges<T>(
            [.. after.Where(e => !old.ContainsKey(e.Key))],
            [.. before.Where(e => !current.ContainsKey(e.Key))],
            [.. after.Where(e => old.TryGetValue(e.Key, out var o) && ContentJson.Serialize(o) != ContentJson.Serialize(e)).Select(e => (old[e.Key], e))]);
    }
}
