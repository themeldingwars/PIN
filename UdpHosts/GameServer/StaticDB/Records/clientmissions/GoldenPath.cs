namespace GameServer.Data.SDB.Records.clientmissions;
public record class GoldenPath
{
    public uint MissionchainId { get; set; }
    public uint MissionId { get; set; }
    public uint DisplayLvl { get; set; }
    public uint LevelReq { get; set; }
    public uint Order { get; set; }
}