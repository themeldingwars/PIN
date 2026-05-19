namespace GameServer.StaticDB.Records.customdata;

public record ResetCooldownsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}