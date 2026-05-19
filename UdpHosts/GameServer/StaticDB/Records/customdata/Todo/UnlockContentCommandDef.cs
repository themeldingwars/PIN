namespace GameServer.StaticDB.Records.customdata;

public record UnlockContentCommandDef : ICommandDef
{
    public uint Id { get; set; }
}