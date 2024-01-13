namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemSetCategoryScalarEntries
{
    public uint NumOwned { get; set; }
    public float Scalar { get; set; }
    public float PerLevel { get; set; }
    public uint AttributeCategory { get; set; }
    public uint ItemSetId { get; set; }
    public uint Id { get; set; }
}
