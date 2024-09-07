namespace GameServer.Data.SDB.Records.customdata;

public record TinyObjectCreateCommandDef : ICommandDef
{
    public uint Id { get; set; }
}