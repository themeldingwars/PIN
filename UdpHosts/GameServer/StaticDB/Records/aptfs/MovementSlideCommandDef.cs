namespace GameServer.Data.SDB.Records.aptfs;
public record class MovementSlideCommandDef
{
    public float OffsetY { get; set; }
    public uint MoveDuration { get; set; }
    public uint RandOffsetRegop { get; set; }
    public float RandOffsetY { get; set; }
    public uint OffsetRegop { get; set; }
    public uint FixedSpeedRegop { get; set; }
    public uint MoveDurationRegop { get; set; }
    public float OffsetX { get; set; }
    public float FixedSpeed { get; set; }
    public float OffsetZ { get; set; }
    public float RandOffsetX { get; set; }
    public float RandOffsetZ { get; set; }
    public uint Id { get; set; }
    public byte VelocityType { get; set; }
    public byte InitiationPosition { get; set; }
    public byte Rollback { get; set; }
    public byte OffsetTarget { get; set; }
    public byte AlongVelocity { get; set; }
    public byte OffsetAim { get; set; }
    public byte OrientationType { get; set; }
    public byte AllowPrediction { get; set; }
}
