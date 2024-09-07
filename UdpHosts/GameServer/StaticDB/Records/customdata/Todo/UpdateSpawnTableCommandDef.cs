namespace GameServer.Data.SDB.Records.customdata;

public record UpdateSpawnTableCommandDef : ICommandDef
{
    public uint Id { get; set; }
}