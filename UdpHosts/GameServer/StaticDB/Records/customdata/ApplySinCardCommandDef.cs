namespace GameServer.StaticDB.Records.customdata;

public record ApplySinCardCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint? Type { get; set; }
}