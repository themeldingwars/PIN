namespace GameServer.StaticDB.Records.customdata;

public record CancelRopePullCommandDef : ICommandDef
{
    public uint Id { get; set; }
}