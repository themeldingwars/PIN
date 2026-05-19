namespace GameServer.StaticDB.Records.customdata;

public record CreateSpawnPointCommandDef : ICommandDef
{
    public uint Id { get; set; }
}