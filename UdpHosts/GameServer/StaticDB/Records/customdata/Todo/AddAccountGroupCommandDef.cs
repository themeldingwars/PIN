namespace GameServer.Data.SDB.Records.customdata;

public record AddAccountGroupCommandDef : ICommandDef
{
    public uint Id { get; set; }
}