using System.Collections.Generic;
using System.Linq;

namespace Shared.Content;

public sealed class ContentKind<T>(string name, bool perZone = true) : ContentKind(name, perZone)
    where T : ContentEntry
{
    public IEnumerable<T> Sort(IEnumerable<T> entries) => entries.OrderBy(entry => entry.Key, NaturalComparer.Instance);
}
