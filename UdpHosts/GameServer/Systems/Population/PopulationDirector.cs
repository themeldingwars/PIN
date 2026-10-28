using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using GameServer.Entities.Character;
using GameServer.Systems.Ai;
using Serilog;
using Shared.Content;
using Shared.Content.Spawns;
using Shared.Content.Zones;

namespace GameServer.Systems.Population;

/// <summary>
///     Keeps the zone's spawn regions at their population. A region only has NPCs while a player is near, so the number of
///     NPCs follows the players and not the size of the map. Killed groups respawn after their table's delay, also while
///     nobody is near.
/// </summary>
public sealed class PopulationDirector
{
    private const ulong _tickIntervalMs = 1000;

    // Bodies that start inside each other get thrown into the air by the physics, a Brontodon is several meters wide
    private const float _memberSpacing = 7f;
    private const int _placementAttempts = 10;

    // How far below and above a polygon's points the navmesh search reaches, hills inside the polygon included
    private const float _polygonNavMeshSearch = 50f;

    private readonly Shard _shard;
    private readonly ILogger _logger;
    private readonly Random _random = new();
    private readonly Dictionary<string, RegionState> _regions = new();
    private ulong _lastTick;
    private bool _started;

    public PopulationDirector(Shard shard)
    {
        _shard = shard;
        _logger = shard.Logger.ForContext<PopulationDirector>();
    }

    public IReadOnlyCollection<RegionState> Regions => _regions.Values;

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Starts with the first real tick, like the zone entities
        if (currentTime == 0 || currentTime < _lastTick + _tickIntervalMs)
        {
            return;
        }

        _lastTick = currentTime;
        if (!_started)
        {
            if (!_shard.Settings.LoadZoneEntities)
            {
                return;
            }

            _started = true;
            foreach (var region in _shard.Content.Regions)
            {
                Add(region);
            }
        }

        var players = _shard.Clients.Values
            .Where(client => client.CanReceiveGSS && client.CharacterEntity != null)
            .Select(client => client.CharacterEntity.Position)
            .ToList();

        foreach (var state in _regions.Values)
        {
            Update(state, players, currentTime);
        }
    }

    public bool Respawn(string regionId)
    {
        if (!_regions.TryGetValue(regionId, out var state))
        {
            return false;
        }

        Despawn(state);
        state.Respawns.Clear();
        return true;
    }

    public void ApplyContentChanges(ZoneContent before, ZoneContent after)
    {
        if (!_started)
        {
            return;
        }

        // The zone's defaults apply to every region that doesn't override them
        if (before.Settings != after.Settings)
        {
            foreach (var id in _regions.Keys.ToList())
            {
                Remove(id);
            }

            foreach (var region in after.Regions)
            {
                Add(region);
            }

            _logger.Information("Population settings of zone {ZoneId} changed, respawned its {Count} regions", _shard.ZoneId, after.Regions.Count);
            return;
        }

        var regions = ContentChanges<SpawnRegion>.Between(before.Regions, after.Regions);
        foreach (var gone in regions.Removed.Concat(regions.Changed.Select(c => c.Before)))
        {
            Remove(gone.Id);
        }

        foreach (var region in regions.Added.Concat(regions.Changed.Select(c => c.After)))
        {
            Add(region);
        }

        if (!regions.IsEmpty)
        {
            _logger.Information(
                "Regions of zone {ZoneId}: {Added} added, {Removed} removed, {Changed} changed",
                _shard.ZoneId,
                regions.Added.Count,
                regions.Removed.Count,
                regions.Changed.Count);
        }
    }

    public void ApplySpawnTableChanges(IReadOnlyDictionary<string, SpawnTable> before, IReadOnlyDictionary<string, SpawnTable> after)
    {
        if (!_started)
        {
            return;
        }

        var changed = ContentChanges<SpawnTable>.Between([.. before.Values], [.. after.Values]).Changed.Select(c => c.After.Id).ToHashSet();
        foreach (var state in _regions.Values.Where(s => changed.Contains(s.Region.Table)).ToList())
        {
            Remove(state.Region.Id);
            Add(state.Region);
        }
    }

    private void Add(SpawnRegion region)
    {
        // The content store doesn't let a table go while a region uses it
        if (!_shard.SpawnTables.TryGetValue(region.Table, out var table))
        {
            _logger.Warning("Region {RegionId} of zone {ZoneId} has no spawn table {TableId}", region.Id, _shard.ZoneId, region.Table);
            return;
        }

        _regions[region.Id] = new RegionState(region, table, _shard.Content.Settings.Population);
    }

    private void Remove(string regionId)
    {
        if (_regions.Remove(regionId, out var state))
        {
            Despawn(state);
        }
    }

    private void Update(RegionState state, List<Vector3> players, ulong now)
    {
        var region = state.Region;
        var reach = state.ActivationRadius + region.Shape.BoundingRadius;
        if (players.Any(position => Vector3.DistanceSquared(position, region.Shape.Middle) <= reach * reach))
        {
            state.LastPlayerNear = now;
            state.IsActive = true;
        }
        else if (state.IsActive && now - state.LastPlayerNear >= state.DespawnAfterMs)
        {
            Despawn(state);
            state.IsActive = false;
        }

        // Dead once every member is, the corpses go away on their own
        for (var i = state.Groups.Count - 1; i >= 0; i--)
        {
            if (state.Groups[i].Members.All(IsGone))
            {
                state.Groups.RemoveAt(i);
                state.Respawns.Add(now + (state.Table.RespawnSeconds.Pick(_random) * 1000UL));
            }
        }

        state.Respawns.RemoveAll(at => at <= now);

        while (state.IsActive && state.Groups.Count + state.Respawns.Count < region.Population)
        {
            state.Groups.Add(SpawnGroup(state));
        }
    }

    private bool IsGone(CharacterEntity npc) => !npc.IsAlive || !_shard.Entities.ContainsKey(npc.EntityId);

    // A despawned group doesn't wait for a respawn, it comes back as soon as a player is near again
    private void Despawn(RegionState state)
    {
        foreach (var npc in state.Groups.SelectMany(group => group.Members).Where(npc => !IsGone(npc)))
        {
            _shard.EntityMan.Remove(npc);
        }

        state.Groups.Clear();
    }

    private NpcGroup SpawnGroup(RegionState state)
    {
        var table = state.Table;
        var group = PickGroup(table);
        var rules = GroupRules.For(group);
        var spot = GroupSpot(state.Region.Shape, rules.Spread);

        var spawned = new NpcGroup(spot, rules);
        var placed = new List<Vector3>();
        foreach (var member in group.Members)
        {
            for (var count = member.Count.Pick(_random); count > 0; count--)
            {
                var level = table.Level is { } levels ? (byte)levels.Pick(_random) : CharacterEntity.DefaultLevel;
                var position = PlaceMember(spot, rules.Spread, placed);
                var npc = _shard.EntityMan.SpawnCharacter(member.Monster, position, level: level);

                // A behavior that wanders on its own keeps this center, the AI only sets one when there's none yet
                var home = rules.SharesHome ? spot : position;
                _shard.AI.Wander(npc, home, rules.Spread);
                spawned.Add(npc, home);
            }
        }

        _logger.Debug("Region {RegionId} spawned {Count} NPCs of {TableId} at {Spot}", state.Region.Id, spawned.Members.Count, table.Id, spot);
        return spawned;
    }

    // The members of a group wander up to its spread from its home, so two homes keep both spreads apart, also across
    // regions. A region too small for that gets the candidate with the most room.
    private Vector3 GroupSpot(RegionShape shape, float spread)
    {
        var others = _regions.Values.SelectMany(state => state.Groups).ToList();
        float Room(Vector3 spot) => others.Count == 0 ? float.MaxValue : others.Min(other => Vector3.Distance(other.Home, spot) - other.Rules.Spread - spread);

        var best = PointIn(shape);
        var bestRoom = Room(best);
        for (var attempt = 1; attempt < _placementAttempts && bestRoom < 0; attempt++)
        {
            var candidate = PointIn(shape);
            var room = Room(candidate);
            if (room > bestRoom)
            {
                best = candidate;
                bestRoom = room;
            }
        }

        return best;
    }

    private SpawnGroup PickGroup(SpawnTable table)
    {
        var roll = _random.NextInt64(table.Groups.Sum(group => (long)group.Weight));
        foreach (var group in table.Groups)
        {
            roll -= group.Weight;
            if (roll < 0)
            {
                return group;
            }
        }

        return table.Groups[^1];
    }

    // Takes the candidate furthest from the members placed so far, if none keeps the spacing
    private Vector3 PlaceMember(Vector3 spot, float spread, List<Vector3> placed)
    {
        var best = spot;
        var bestOnNavMesh = false;
        var bestGap = -1f;
        for (var attempt = 0; attempt < _placementAttempts && bestGap < _memberSpacing; attempt++)
        {
            var candidate = PointNear(spot, spread, out var onNavMesh);
            var gap = placed.Count == 0 ? float.MaxValue : placed.Min(other => Vector3.Distance(other, candidate));
            if (gap > bestGap)
            {
                best = candidate;
                bestOnNavMesh = onNavMesh;
                bestGap = gap;
            }
        }

        placed.Add(best);
        return Ground(best, bestOnNavMesh);
    }

    private Vector3 PointIn(RegionShape shape)
    {
        return shape switch
        {
            CircleShape circle => PointNear(circle.Center, circle.Radius, out _),
            PolygonShape polygon => PointIn(polygon),
            _ => shape.Middle,
        };
    }

    // A random spot inside, moved onto the navmesh where there is one. A concave polygon can miss a few times, the
    // middle is the fallback, which may lie outside such a polygon.
    private Vector3 PointIn(PolygonShape polygon)
    {
        var (min, max) = polygon.Bounds();
        var height = polygon.Middle.Z;
        for (var attempt = 0; attempt < _placementAttempts; attempt++)
        {
            var point = new Vector2(min.X + (_random.NextSingle() * (max.X - min.X)), min.Y + (_random.NextSingle() * (max.Y - min.Y)));
            if (!polygon.Contains(point))
            {
                continue;
            }

            var candidate = new Vector3(point, height);
            return _shard.NavMesh.FindNearestPoly(candidate, _polygonNavMeshSearch, out _, out var onMesh) ? onMesh : candidate;
        }

        return polygon.Middle;
    }

    private Vector3 PointNear(Vector3 center, float radius, out bool onNavMesh)
    {
        onNavMesh = false;
        if (radius <= 0)
        {
            return center;
        }

        if (_shard.NavMesh.TryFindRandomPointWithinCircle(center, radius, out var point))
        {
            onNavMesh = true;
            return point;
        }

        // No navmesh around, the ground check still finds the terrain
        var angle = _random.NextSingle() * MathF.Tau;
        var distance = radius * MathF.Sqrt(_random.NextSingle());
        return center + new Vector3(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance, 0);
    }

    // Lifted off the surface like the npc command does, a body that starts inside the ground gets pushed around. A navmesh
    // point is on the ground already, a long cast from above it would land on rocks and trees. Without a navmesh the
    // point has the region's height, the terrain can be well above or below it.
    private Vector3 Ground(Vector3 point, bool onNavMesh)
    {
        var lift = onNavMesh ? 1.5f : 30f;
        return _shard.Physics.TryFindSurfaceSpawn(point + (Vector3.UnitZ * lift), -Vector3.UnitZ, lift * 2, 0.5f, 0.5f, 0, out var grounded)
            ? grounded
            : point + (Vector3.UnitZ * 0.5f);
    }
}
