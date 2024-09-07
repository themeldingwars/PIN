namespace GameServer.Data.SDB.Records.customdata;

public record RequireInitiatorExistsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}