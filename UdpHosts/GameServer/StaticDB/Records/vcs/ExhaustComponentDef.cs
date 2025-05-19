namespace GameServer.Data.SDB.Records.vcs;
public record class ExhaustComponentDef
{
    public uint UpshiftEffectId { get; set; }
    public uint DownshiftEffectId { get; set; }
    public string Hardpoint { get; set; }
    public uint EffectId { get; set; }
    public uint Id { get; set; }
}