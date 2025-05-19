namespace GameServer.Data.SDB.Records.clientmissions;
public record class AchievementSetLink
{
    public uint AchievementId { get; set; }
    public uint AchievementsetId { get; set; }
    public uint SortOrder { get; set; }
}