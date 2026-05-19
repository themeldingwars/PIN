namespace GameServer.StaticDB.Records.customdata;

public record TeleportCommandDef : ICommandDef
{
    public uint Id { get; set; }
}