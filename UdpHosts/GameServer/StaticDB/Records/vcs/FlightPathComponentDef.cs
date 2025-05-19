namespace GameServer.Data.SDB.Records.vcs;
public record class FlightPathComponentDef
{
    public float LiftoffSpeed { get; set; }
    public float LiftoffAccel { get; set; }
    public float PauseAngle { get; set; }
    public float FlyingAccel { get; set; }
    public float LandingSpeedMin { get; set; }
    public float RotationSmoothing { get; set; }
    public float PauseDecelTime { get; set; }
    public float AlternateFlyingSpeed { get; set; }
    public uint LandingEffectId { get; set; }
    public uint LandingPoseFile { get; set; }
    public float LandingSpeedMax { get; set; }
    public uint Id { get; set; }
    public float FlyingSpeed { get; set; }
    public float MaxRotationRate { get; set; }
}