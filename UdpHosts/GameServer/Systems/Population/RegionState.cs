using System.Collections.Generic;
using GameServer.Systems.Ai;
using Shared.Content.Spawns;
using Shared.Content.Zones;

namespace GameServer.Systems.Population;

public sealed class RegionState(SpawnRegion region, SpawnTable table, PopulationSettings zone)
{
    public SpawnRegion Region { get; } = region;

    public SpawnTable Table { get; } = table;

    /// <summary>The region's own value, else the zone's.</summary>
    public float ActivationRadius { get; } = region.ActivationRadius ?? zone.ActivationRadius;

    /// <summary>The region's own value, else the zone's.</summary>
    public ulong DespawnAfterMs { get; } = (region.DespawnAfterSeconds ?? zone.DespawnAfterSeconds) * 1000UL;

    public bool IsActive { get; internal set; }

    public List<NpcGroup> Groups { get; } = [];

    public List<ulong> Respawns { get; } = [];

    internal ulong LastPlayerNear { get; set; }
}
