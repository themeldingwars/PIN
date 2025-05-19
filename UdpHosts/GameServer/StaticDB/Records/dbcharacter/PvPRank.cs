namespace GameServer.Data.SDB.Records.dbcharacter;
public record class PvPRank
{
    public uint NameId { get; set; }
    public uint PerkPoints { get; set; }
    public uint PerkCertificate { get; set; }
    public uint Rank { get; set; }
    public uint RequiredRankPoints { get; set; }
    public uint RewardLootTable { get; set; }
}