namespace GameServer.Data.SDB.Records.aptfs;
public record class OrientationLockCommandDef
{
    public float MaxAimAngle { get; set; }
    public uint Duration { get; set; }
    public uint Id { get; set; }
    public byte OrientationType { get; set; }
    public byte AllowPrediction { get; set; }
}
