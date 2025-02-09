namespace GameServer.Data.SDB.Records.customdata;

public record TeleportInstanceCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; } = 0;
}