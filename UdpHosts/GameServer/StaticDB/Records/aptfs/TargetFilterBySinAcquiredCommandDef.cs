namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetFilterBySinAcquiredCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte FailNoTargets { get; set; }
    public byte Negate { get; set; }
}
