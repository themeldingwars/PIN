namespace GameServer.Data.SDB.Records.customdata;

public record ApplyUnlockCommandDef : ICommandDef
{
    public uint Id { get; set; }
}