namespace GameServer.StaticDB.Records.customdata;

public record SetRespawnFlagsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}