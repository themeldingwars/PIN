namespace GameServer.Data.SDB.Records.customdata;

public record RequestArcJobsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}