namespace GameServer.StaticDB.Records.customdata;

public record ForceRespawnCommandDef : ICommandDef
{
    public uint Id { get; set; }
}