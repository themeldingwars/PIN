namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetByEffectTagCommandDef
{
    public uint StackCount { get; set; }
    public uint TagId { get; set; }
    public uint Id { get; set; }
    public byte FailNoTargets { get; set; }
    public byte Negate { get; set; }
}
