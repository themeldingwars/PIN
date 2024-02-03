using System;
using System.Numerics;

namespace GameServer.Data.SDB.Records.customdata;
public record class Melding
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }
    public string PerimiterSetName { get; set; }
    public uint Unk1 { get; set; }
    public uint Unk2 { get; set; }
    public byte Unk3 { get; set; }
    public Vector3[] ControlPoints { get; set; } = Array.Empty<Vector3>();
    public Vector3[] Offsets { get; set; } = Array.Empty<Vector3>();
}