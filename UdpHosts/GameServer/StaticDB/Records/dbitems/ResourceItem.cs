namespace GameServer.StaticDB.Records.dbitems;
public record class ResourceItem
{
    public uint RefinesInto { get; set; }
    public uint Id { get; set; }
}