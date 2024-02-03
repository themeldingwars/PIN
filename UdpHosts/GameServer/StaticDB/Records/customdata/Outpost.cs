using System;
using System.Numerics;

namespace GameServer.Data.SDB.Records.customdata;
public record class Outpost
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }

    public uint OutpostName { get; set; }
    public Vector3 Position { get; set; }
    public uint LevelBandId { get; set; }
    public byte SinUnlockIndex { get; set; }
    public int TeleportCost { get; set; }
    public byte FactionId { get; set; }
    public byte OutpostType { get; set; }
    public uint PossibleBuffsId { get; set; }
    public float Radius { get; set; }
    public uint MarkerType { get; set; }
}