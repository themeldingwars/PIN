namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemSetAttributeRangeEntries
{
    public uint AttributeId { get; set; }
    public uint NumOwned { get; set; }
    public float Base { get; set; }
    public float PerLevel { get; set; }
    public uint ItemSetId { get; set; }
    public uint Id { get; set; }
}
