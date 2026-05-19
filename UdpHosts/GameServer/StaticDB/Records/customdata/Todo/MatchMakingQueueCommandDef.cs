namespace GameServer.StaticDB.Records.customdata;

public record MatchMakingQueueCommandDef : ICommandDef
{
    public uint Id { get; set; }
}