using System.Numerics;

namespace GameServer.Systems.Loot;

/// <summary>
/// A rolled item that is sitting on the ground in an AreaVisualData loot slot.
/// </summary>
public sealed record LootDropInfo(
    uint SdbId,
    uint Quantity,
    Vector3 Position,
    ulong DespawnAt);
