namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireEliteLevelCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort Level { get; set; }
}
