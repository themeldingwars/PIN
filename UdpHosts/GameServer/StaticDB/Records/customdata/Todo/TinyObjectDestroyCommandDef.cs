namespace GameServer.StaticDB.Records.customdata;

public record TinyObjectDestroyCommandDef : ICommandDef
{
    public uint Id { get; set; }
}