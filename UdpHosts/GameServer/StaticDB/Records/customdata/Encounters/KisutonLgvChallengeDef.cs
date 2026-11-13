using System.Collections.Generic;
using System.Numerics;

namespace GameServer.StaticDB.Records.customdata.Encounters;

public record KisutonLgvChallengeDef : IEncounterDef
{
    public uint ZoneId { get; set; }
    public ushort Type { get; set; }
    public uint ChallengeName { get; set; }
    public uint TrackName { get; set; }
    public ushort Laps { get; set; }
    public uint LeaderboardId { get; set; }
    public SpawnPose Terminal { get; set; }
    public SpawnPose StartPosition { get; set; }
    public SpawnPose LeaderboardTerminal { get; set; }
    public List<Vector3> Waypoints { get; set; }
}
