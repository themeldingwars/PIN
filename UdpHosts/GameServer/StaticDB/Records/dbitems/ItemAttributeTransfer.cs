namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemAttributeTransfer
{
    public uint ObjectId { get; set; }
    public uint FromStat { get; set; }
    public uint ToStat { get; set; }
}