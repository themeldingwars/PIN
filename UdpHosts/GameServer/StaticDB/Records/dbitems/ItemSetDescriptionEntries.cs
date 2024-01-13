namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemSetDescriptionEntries
{

    public uint NumOwned { get; set; }
    public uint LocalizedDescId { get; set; }
    public uint ItemSetId { get; set; }
    public uint Id { get; set; }
}
