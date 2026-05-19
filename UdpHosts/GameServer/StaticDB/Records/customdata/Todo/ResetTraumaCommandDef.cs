namespace GameServer.StaticDB.Records.customdata;

public record ResetTraumaCommandDef : ICommandDef
{
    public uint Id { get; set; }
}