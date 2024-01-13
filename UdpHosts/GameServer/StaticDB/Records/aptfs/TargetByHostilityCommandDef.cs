namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetByHostilityCommandDef
{
    public uint Id { get; set; }
    public byte IncludeInitiator { get; set; }
    public byte IncludeOwner { get; set; }
    public byte CompareFromInitiator { get; set; }
    public byte IncludeNormalOnly { get; set; }
    public byte FailNoTargets { get; set; }
    public byte FilterType { get; set; }
    public byte IncludeSelf { get; set; }
    public byte ExcludeMode { get; set; }
}
