namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemModuleSlots
{
    public uint ItemId { get; set; }
    public byte SlotColor { get; set; }
    public byte SlotIndex { get; set; }
}