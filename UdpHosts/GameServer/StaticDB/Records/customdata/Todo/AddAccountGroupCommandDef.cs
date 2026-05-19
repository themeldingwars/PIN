namespace GameServer.StaticDB.Records.customdata;

public record AddAccountGroupCommandDef : ICommandDef
{
    public uint Id { get; set; }
}