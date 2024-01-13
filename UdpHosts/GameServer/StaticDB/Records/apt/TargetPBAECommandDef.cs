namespace GameServer.Data.SDB.Records.apt;
public record class TargetPBAECommandDef
{
    // public Vec3 AimPosOffset { get; set; }
    public float Radius { get; set; }
    public uint Id { get; set; }
    public ushort MaxTargets { get; set; }
    public byte UseInitPos { get; set; }
    public byte MinTargets { get; set; }
    public byte ScaleOffset { get; set; }
    public byte RadiusRegop { get; set; }
    public byte IncludeInteractives { get; set; }
    public byte IgnoreWalls { get; set; }
    public byte UseWeaponRadius { get; set; }
    public byte ScaleQuerySize { get; set; }
    public byte Filter { get; set; }
    public byte UseBodyPosition { get; set; }
    public byte IncludeSelf { get; set; }
}