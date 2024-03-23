using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Data.SDB.Records.customdata;
public record Outpost
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }

    public uint OutpostName { get; set; }
    public Vector3 Position { get; set; }
    public List<SpawnPoint> SpawnPoints { get; set; } = [];
    public uint LevelBandId { get; set; }
    public byte SinUnlockIndex { get; set; }
    public int TeleportCost { get; set; }
    public byte FactionId { get; set; }
    public byte OutpostType { get; set; }
    public uint PossibleBuffsId { get; set; }
    public float Radius { get; set; }
    public uint MarkerType { get; set; }
}

public record SpawnPoint
{
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; } = Quaternion.Identity;
    public Vector3 AimDirection { get; set; } = Vector3.Zero;
}