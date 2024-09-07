namespace GameServer.Data.SDB.Records.customdata;

public record CalculateTrajectoryCommandDef : ICommandDef
{
    public uint Id { get; set; }
}