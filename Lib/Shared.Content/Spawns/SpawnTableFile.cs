using System.Collections.Generic;

namespace Shared.Content.Spawns;

/// <summary>A file in <c>spawn-tables/</c>, named after the family of its tables, e.g. <c>brontodons</c>.</summary>
public sealed record SpawnTableFile(string Family, IReadOnlyList<SpawnTable> Tables);
