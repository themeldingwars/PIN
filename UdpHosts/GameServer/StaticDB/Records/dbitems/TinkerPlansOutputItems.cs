namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlansOutputItems
{
    public uint TinkerplansId { get; set; }
    public uint ItemId { get; set; }
    public string FeatureEnabled { get; set; }
    public byte IsGear { get; set; }
    public sbyte RankOffset { get; set; }
    public sbyte RankChannel { get; set; }
    public byte CriticalType { get; set; }
}