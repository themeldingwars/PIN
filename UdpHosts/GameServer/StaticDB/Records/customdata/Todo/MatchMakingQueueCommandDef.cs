namespace GameServer.Data.SDB.Records.customdata;

public record MatchMakingQueueCommandDef : ICommandDef
{
    public uint Id { get; set; }
}