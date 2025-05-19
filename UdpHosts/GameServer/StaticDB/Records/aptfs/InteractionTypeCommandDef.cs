namespace GameServer.Data.SDB.Records.aptfs;
public record class InteractionTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Type { get; set; }
}