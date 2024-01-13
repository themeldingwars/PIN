namespace GameServer.Data.SDB.Records.clientmissions;
public record class MissionChains
{
    public uint MissionchainId { get; set; }
    public uint MissionId { get; set; }
    public uint Order { get; set; }
}
