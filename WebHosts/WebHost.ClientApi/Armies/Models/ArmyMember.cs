namespace WebHost.ClientApi.Armies.Models;

public class ArmyMember
{
    public uint? Id { get; set; }
    public uint? ArmyId { get; set; }
    public ulong ArmyGuid { get; set; }
    public ulong CharacterGuid { get; set; }
    public uint ArmyRankId { get; set; }
    public uint CreatedAt { get; set; }
    public uint UpdatedAt { get; set; }
    public string RankName { get; set; }
    public uint RankPosition { get; set; }
    public uint LastZoneId { get; set; }
    public uint LastSeenAt { get; set; }
    public uint? CurrentLevel { get; set; }
    public uint? CurrentFrameSdbId { get; set; }
    public bool? IsOnline { get; set; }
    public string PublicNote { get; set; }
    public string OfficerNote { get; set; }
    public string Name { get; set; }
}