namespace GameServer.StaticDB.Records.customdata;

public record ActivateSpawnTableCommandDef : ICommandDef
{
    public uint Id { get; set; }
}