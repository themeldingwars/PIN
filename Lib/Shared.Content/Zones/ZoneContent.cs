using System.Collections.Generic;
using Shared.Content.Zones.Encounters;

namespace Shared.Content.Zones;

/// <summary>Everything a zone's content files place in it. A missing file means none of that kind.</summary>
public sealed record ZoneContent(uint ZoneId)
{
    public IReadOnlyList<ZoneDeployable> Deployables { get; init; } = [];

    public IReadOnlyList<ZoneOutpost> Outposts { get; init; } = [];

    public IReadOnlyList<ZoneMelding> Meldings { get; init; } = [];

    public IReadOnlyList<ZoneEncounter> Encounters { get; init; } = [];
}
