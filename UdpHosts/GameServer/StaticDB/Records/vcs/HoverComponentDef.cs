namespace GameServer.Data.SDB.Records.vcs;
public record class HoverComponentDef
{
    public float MaxLateralSpeed { get; set; }
    public float MaxAngularVelocity { get; set; }
    public float MaxPitch { get; set; }
    public float MaxForwardSpeed { get; set; }
    public float MaxRoll { get; set; }
    public uint Id { get; set; }
}
