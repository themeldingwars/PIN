namespace GameServer.Data.SDB.Records.clientmissions;
public record class MissionRewards
{
    public uint MissionId { get; set; }
    public uint Quantity { get; set; }
    public uint ItemId { get; set; }
}