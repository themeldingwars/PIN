namespace GameServer.Data.SDB.Records.customdata;

public record CancelRopePullCommandDef : ICommandDef
{
    public uint Id { get; set; }
}