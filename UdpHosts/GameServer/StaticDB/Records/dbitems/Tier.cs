namespace GameServer.Data.SDB.Records.dbitems;
public record class TierRecord
{
    public uint LocalizedNameId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint Id { get; set; }
    public int Stage { get; set; }
    public byte Type { get; set; }
    public byte Tier { get; set; }
}