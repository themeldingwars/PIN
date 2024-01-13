namespace GameServer.Data.SDB.Records.vcs;
public record class LightComponentDef
{
    public uint LightType { get; set; }
    public string Hardpoint { get; set; }
    public uint EffectId { get; set; }
    public uint Id { get; set; }
}
