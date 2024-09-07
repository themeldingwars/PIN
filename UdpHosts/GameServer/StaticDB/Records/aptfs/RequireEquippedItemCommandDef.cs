namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireEquippedItemCommandDef : ICommandDef
{
    public uint ItemId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
