namespace GameServer.Data.SDB.Records.customdata;

public record TinyObjectDestroyCommandDef : ICommandDef
{
    public uint Id { get; set; }
}