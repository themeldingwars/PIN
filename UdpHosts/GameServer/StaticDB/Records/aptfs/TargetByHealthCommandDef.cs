namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetByHealthCommandDef : ICommandDef
{
    public int HealthPct { get; set; }
    public uint Id { get; set; }
    public byte FailNoTargets { get; set; }
    public byte HealthRegop { get; set; }
    public byte Negate { get; set; }
}
