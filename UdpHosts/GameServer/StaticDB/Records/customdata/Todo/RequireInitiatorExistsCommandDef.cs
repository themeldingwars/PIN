namespace GameServer.StaticDB.Records.customdata;

public record RequireInitiatorExistsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}