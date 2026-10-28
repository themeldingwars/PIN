using System.Collections.Generic;
using Shared.Content.Zones;
using Shared.Content.Zones.Encounters;

namespace Shared.Content;

public static class ContentKinds
{
    public static readonly ContentKind<ZoneDeployable> Deployables = new("deployables");
    public static readonly ContentKind<ZoneOutpost> Outposts = new("outposts");
    public static readonly ContentKind<ZoneMelding> Meldings = new("meldings");
    public static readonly ContentKind<ZoneEncounter> Encounters = new("encounters");

    public static IReadOnlyList<ContentKind> All { get; } = [Deployables, Outposts, Meldings, Encounters];
}
