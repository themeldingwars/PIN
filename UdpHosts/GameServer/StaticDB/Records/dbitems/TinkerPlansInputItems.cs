namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlansInputItems
{
    public uint TinkerplansId { get; set; }
    public int Quantity { get; set; }
    public uint ItemId { get; set; }
    public string FeatureEnabled { get; set; }
    public byte IsGear { get; set; }
    public sbyte RankOffset { get; set; }
    public sbyte RankChannel { get; set; }
    public byte DetectRank { get; set; }
}
