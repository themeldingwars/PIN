namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireSuperChargeCommandDef
{
    public float Percent { get; set; }
    public uint Id { get; set; }
    public byte PercentRegop { get; set; }
    public byte Negate { get; set; }
}
