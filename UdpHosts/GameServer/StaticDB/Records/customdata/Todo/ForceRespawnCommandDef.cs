namespace GameServer.Data.SDB.Records.customdata;

public record ForceRespawnCommandDef : ICommandDef
{
    public uint Id { get; set; }
}