namespace GameServer.Data.SDB.Records.customdata;

public record UnlockContentCommandDef : ICommandDef
{
    public uint Id { get; set; }
}