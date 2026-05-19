namespace GameServer.StaticDB.Records.customdata;

public record ApplyUnlockCommandDef : ICommandDef
{
    public uint Id { get; set; }
}