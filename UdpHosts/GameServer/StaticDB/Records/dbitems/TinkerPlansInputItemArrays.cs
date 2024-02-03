namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlansInputItemArrays
{
    public uint TinkerplansId { get; set; }
    public string FeatureEnabled { get; set; }
    public uint ItemarrayId { get; set; }
    public sbyte RankOffset { get; set; }
    public sbyte RankChannel { get; set; }
    public sbyte SlotIndex { get; set; }
    public byte DetectRank { get; set; }
}
