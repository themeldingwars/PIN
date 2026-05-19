namespace GameServer.StaticDB.Records.customdata;

public record UpdateSpawnTableCommandDef : ICommandDef
{
    public uint Id { get; set; }
}