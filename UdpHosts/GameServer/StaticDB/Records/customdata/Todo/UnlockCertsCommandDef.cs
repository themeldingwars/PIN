namespace GameServer.Data.SDB.Records.customdata;

public record UnlockCertsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}