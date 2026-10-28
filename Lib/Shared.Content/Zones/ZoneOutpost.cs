using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Shared.Content.Zones;

/// <summary>An outpost keeps the numeric id the game uses, the client and RIN refer to outposts by it.</summary>
public sealed record ZoneOutpost : ContentEntry
{
    public required uint Id { get; init; }

    /// <summary>The localized name, an SDB text id.</summary>
    public required uint NameTextId { get; init; }

    public required Vector3 Position { get; init; }

    public float Radius { get; init; }

    public uint LevelBandId { get; init; }

    public byte SinUnlockIndex { get; init; }

    public int TeleportCost { get; init; }

    public byte FactionId { get; init; }

    public byte OutpostType { get; init; }

    public uint PossibleBuffsId { get; init; }

    public uint MarkerType { get; init; }

    public IReadOnlyList<OutpostSpawnPoint> SpawnPoints { get; init; } = [];

    [JsonIgnore]
    public override string Key => Id.ToString(CultureInfo.InvariantCulture);
}
