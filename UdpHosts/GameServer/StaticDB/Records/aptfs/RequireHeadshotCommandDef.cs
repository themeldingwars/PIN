namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireHeadshotCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
