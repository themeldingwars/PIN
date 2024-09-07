namespace GameServer.Data.SDB.Records.customdata;

public record CreateSpawnPointCommandDef : ICommandDef
{
    public uint Id { get; set; }
}