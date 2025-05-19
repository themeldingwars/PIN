namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireHasUnlockCommandDef : ICommandDef
{
    public string UnlockType { get; set; }
    public uint UnlockId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}