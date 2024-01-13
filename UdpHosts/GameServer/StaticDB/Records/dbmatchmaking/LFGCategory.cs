namespace GameServer.Data.SDB.Records.dbmatchmaking;
public record class LFGCategory
{
    public uint LocalizedNameId { get; set; }
    public uint ParentId { get; set; }
    public uint Id { get; set; }
}
