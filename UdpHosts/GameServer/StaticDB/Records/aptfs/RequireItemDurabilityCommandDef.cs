namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireItemDurabilityCommandDef
{
    public uint Id { get; set; }
    public ushort DurabilityAmount { get; set; }
    public byte SlotType { get; set; }
    public byte EqualTo { get; set; }
    public byte LessThan { get; set; }
    public byte GreaterThan { get; set; }
}
