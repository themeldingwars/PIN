namespace GameServer.Data.SDB.Records.vcs;
public record class HelicopterComponentDef
{
    public float MaxLateralSpeed { get; set; }
    public float MaxAngularVelocity { get; set; }
    public float MaxPitch { get; set; }
    public float MaxForwardSpeed { get; set; }
    public float MaxRoll { get; set; }
    public float MaxElevation { get; set; }
    public uint Id { get; set; }
}
