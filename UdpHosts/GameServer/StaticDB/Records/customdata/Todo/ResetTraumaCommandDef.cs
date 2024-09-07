namespace GameServer.Data.SDB.Records.customdata;

public record ResetTraumaCommandDef : ICommandDef
{
    public uint Id { get; set; }
}