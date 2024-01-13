namespace GameServer.Data.SDB.Records.dbitems;
public record class LootStreakEntries
{
    public uint ResourceId { get; set; }
    public uint StreakLen { get; set; }
    public uint LootstreakId { get; set; }
    public uint Amount { get; set; }
    public uint LootTableId { get; set; }
}
