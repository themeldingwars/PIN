namespace GameServer.StaticDB.Records.customdata;

public record RequestArcJobsCommandDef : ICommandDef
{
    public uint Id { get; set; }
}