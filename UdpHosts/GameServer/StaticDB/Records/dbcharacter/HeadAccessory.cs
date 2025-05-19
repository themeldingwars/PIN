namespace GameServer.Data.SDB.Records.dbcharacter;
public record class HeadAccessory
{
    public uint LocNameId { get; set; }
    public uint VisualrecId { get; set; }
    public uint HaId { get; set; }
    public string Sex { get; set; }
    public byte UnlockedByDefault { get; set; }
    public byte SlotId { get; set; }
    public byte ExcludeFromRelease { get; set; }
}