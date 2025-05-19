namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CharCreateLoadoutSlots
{
    public uint LoadoutId { get; set; }
    public uint DefaultPvpModule { get; set; }
    public uint DefaultPveModule { get; set; }
    public uint DebugRefModule { get; set; }
    public byte SlotType { get; set; }
}