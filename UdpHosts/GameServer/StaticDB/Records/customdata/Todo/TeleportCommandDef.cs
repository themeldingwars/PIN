namespace GameServer.Data.SDB.Records.customdata;

public record TeleportCommandDef : ICommandDef
{
    public uint Id { get; set; }
}