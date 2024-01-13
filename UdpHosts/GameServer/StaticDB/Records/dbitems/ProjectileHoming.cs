namespace GameServer.Data.SDB.Records.dbitems;
public record class ProjectileHoming
{
    public float AvoidanceAngle { get; set; }
    public float Acceleration { get; set; }
    public float AttackSpeed { get; set; }
    public float MaxAltitude { get; set; }
    public float MinAltitude { get; set; }
    public float LeanThrust { get; set; }
    public float AvoidanceFreq { get; set; }
    public uint DetectionPeriod { get; set; }
    public float CruiseSpeed { get; set; }
    public float AvoidanceRange { get; set; }
    public float SteerThrust { get; set; }
    public uint Id { get; set; }
    public float DetectionRange { get; set; }
}
