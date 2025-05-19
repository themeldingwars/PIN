namespace GameServer.Data.SDB.Records.vcs;
public record class SimpleShieldGeneratorComponentDef
{
    public uint ShieldEffectId { get; set; }
    public string Hardpoint { get; set; }
    public uint Id { get; set; }
}