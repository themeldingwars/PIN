namespace GameServer.Data.SDB.Records.dbitems;
public record class Resource_Stat_Names
{
    public uint LocalizedNameId { get; set; }
    public uint ResourceType { get; set; }
    public uint StatIndex { get; set; }
    public uint Id { get; set; }
}