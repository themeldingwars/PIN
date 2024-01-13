namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlans
{
    public uint LocalizedNameId { get; set; }
    public float BaseCriticalChance { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint Id { get; set; }
    public short MaxRank { get; set; }
    public short MinRank { get; set; }
}
