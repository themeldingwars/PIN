namespace GameServer.StaticDB.Records.customdata;

public record TinyObjectCreateCommandDef : ICommandDef
{
    public uint Id { get; set; }
}