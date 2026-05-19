namespace GameServer.StaticDB.Records.customdata;

public record CalculateTrajectoryCommandDef : ICommandDef
{
    public uint Id { get; set; }
}