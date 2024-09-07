namespace GameServer.Data.SDB.Records.customdata;

public record TinyObjectUpdateCommandDef : ICommandDef
{
    public uint Id { get; set; }
}