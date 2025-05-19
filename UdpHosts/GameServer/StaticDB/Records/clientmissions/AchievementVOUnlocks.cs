namespace GameServer.Data.SDB.Records.clientmissions;
public record class AchievementVOUnlocks
{
    public uint MissionId { get; set; }
    public uint DialogScriptId { get; set; }
    public uint Order { get; set; }
}