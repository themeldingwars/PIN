namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireHasItemCommandDef
{
    public uint Quantity { get; set; }
    public uint ItemId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
