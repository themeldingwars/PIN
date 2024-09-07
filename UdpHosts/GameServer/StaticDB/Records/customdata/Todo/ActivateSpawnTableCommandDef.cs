namespace GameServer.Data.SDB.Records.customdata;

public record ActivateSpawnTableCommandDef : ICommandDef
{
    public uint Id { get; set; }
}