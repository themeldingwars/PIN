namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlansOutputItemArrays
{
    public uint TinkerplansId { get; set; }
    public string? FeatureEnabled { get; set; }
    public uint ItemarrayId { get; set; }
    public sbyte RankOffset { get; set; }
    public sbyte RankChannel { get; set; }
    public sbyte SlotIndex { get; set; }
    public byte CriticalType { get; set; }
}
