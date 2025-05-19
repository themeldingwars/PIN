namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetByEffectCommandDef : ICommandDef
{
    public uint StackCount { get; set; }
    public uint EffectId { get; set; }
    public uint Id { get; set; }
    public byte FailNoTargets { get; set; }
    public byte SameInitiator { get; set; }
    public byte FilterList { get; set; }
    public byte Negate { get; set; }
}