namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemSet
{
    public uint ItemSetId { get; set; }
    public uint LocalizedNameId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
}