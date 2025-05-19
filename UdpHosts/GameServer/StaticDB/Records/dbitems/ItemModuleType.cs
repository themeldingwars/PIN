namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemModuleType
{
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public byte Color { get; set; }
}