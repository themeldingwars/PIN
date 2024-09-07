namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireSuperChargeCommandDef : ICommandDef
{
    public float Percent { get; set; }
    public uint Id { get; set; }
    public byte PercentRegop { get; set; }
    public byte Negate { get; set; }
}
