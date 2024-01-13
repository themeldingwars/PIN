namespace GameServer.Data.SDB.Records.vcs;
public record class ShieldGeneratorComponentDef
{
    public int RechargeRate { get; set; }
    public uint RechargeDelay { get; set; }
    public float Capacity { get; set; }
    public float Radius { get; set; }
    public uint Id { get; set; }
}
