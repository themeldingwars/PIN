namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetFilterByRangeCommandDef : ICommandDef
{
    public float Range { get; set; }
    public uint Id { get; set; }
    public byte RangeRegop { get; set; }
    public byte FailNoTargets { get; set; }
    public byte Negate { get; set; }
}
