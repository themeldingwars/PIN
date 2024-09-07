namespace GameServer.Data.SDB.Records.customdata;

public record ResetCooldownsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}