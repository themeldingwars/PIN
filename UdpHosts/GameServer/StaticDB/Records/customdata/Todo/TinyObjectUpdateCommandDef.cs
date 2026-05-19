namespace GameServer.StaticDB.Records.customdata;

public record TinyObjectUpdateCommandDef : ICommandDef
{
    public uint Id { get; set; }
}