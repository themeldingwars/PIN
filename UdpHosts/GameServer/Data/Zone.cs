using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Data;

public class Zone
{
    // from client's Game.GetZoneInfo(zoneId)
    public static readonly HashSet<uint> OpenWorldZones = new() { 162, 448, 1030, 1125 };
    public static readonly HashSet<uint> HolmgangZones = new() { 844, 1147 };
    public static readonly HashSet<uint> AdventureZones = new()
    {
        803, 805, 833, 864, 865, 868, 1003, 1007, 1008, 1051, 1069, 1089, 1093, 1100, 1101, 1102, 1104, 1106,
        1113, 1114, 1117, 1134, 1151, 1154, 1155, 1162, 1163, 1171, 1173, 1181
    };
    public static readonly HashSet<uint> OtherZones = new() { 12 };


    public Zone()
    {
        POIs = new Dictionary<string, Vector3>();
    }

    public uint ID { get; set; }
    public string Name { get; set; }
    public ulong Timestamp { get; set; }
    public Dictionary<string, Vector3> POIs { get; protected set; }
    public uint DefaultOutpostId { get; set; }
    public bool IsOpenWorld => OpenWorldZones.Contains(ID);
}