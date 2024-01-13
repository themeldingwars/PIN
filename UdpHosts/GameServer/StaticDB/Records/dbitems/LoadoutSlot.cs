namespace GameServer.Data.SDB.Records.dbitems;
public record class LoadoutSlot
{
    public uint ItemLocation { get; set; }
    public uint CertRequirement { get; set; }
    public uint AcceptedItemType { get; set; }
    public uint AcceptedItemTypeMetadata { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte LoadoutModuleType { get; set; }
    public byte LevelRequirement { get; set; }
}
