namespace GameServer.Data.SDB.Records.customdata;

public record SetRespawnFlagsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}