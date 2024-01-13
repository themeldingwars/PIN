namespace GameServer.Data.SDB.Records.apt;
public record class TargetConeAECommandDef
{
    // public Vec3 AimDirOffset { get; set; }
    // public Vec3 AimPosOffset { get; set; }
    public float Range { get; set; }
    public float Angle { get; set; }
    public float MinRadius { get; set; }
    public float MaxRadius { get; set; }
    public float AimRadiusBias { get; set; }
    public uint Id { get; set; }
    public byte UseInitPos { get; set; }
    public byte MinTargets { get; set; }
    public byte ScaleOffset { get; set; }
    public byte RadiusRegop { get; set; }
    public byte IncludeInteractives { get; set; }
    public byte IgnoreWalls { get; set; }
    public byte RangeRegop { get; set; }
    public byte ScaleQuerySize { get; set; }
    public byte UseBodyOrient { get; set; }
    public byte Filter { get; set; }
    public byte UseBodyPosition { get; set; }
    public byte IgnorePastEndpoints { get; set; }
    public byte MaxTargets { get; set; }
    public byte SortByAngle { get; set; }
}