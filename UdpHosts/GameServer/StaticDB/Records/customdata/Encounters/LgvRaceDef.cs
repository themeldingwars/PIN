using System.Numerics;

namespace GameServer.StaticDB.Records.customdata.Encounters;

public record LgvRaceDef : IEncounterDef
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }
    public uint LeaderboardId { get; set; }
    public uint Label { get; set; }
    public uint StartDialog { get; set; }
    public SpawnPose Start { get; set; }
    public SpawnPose Finish { get; set; }
    public SpawnPose Terminal { get; set; }
    public uint TimeLimitMs { get; set; }
    public uint BonusTimeLimitMs { get; set; }
}

public record SpawnPose
{
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }
}
