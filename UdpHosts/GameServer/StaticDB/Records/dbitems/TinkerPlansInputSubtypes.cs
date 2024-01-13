namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerPlansInputSubtypes
{
    public uint TinkerplansId { get; set; }
    public string? FeatureEnabled { get; set; }
    public uint SubtypeId { get; set; }
    public byte IsGear { get; set; }
    public sbyte RankOffset { get; set; }
    public sbyte RankChannel { get; set; }
    public byte IsOptional { get; set; }
    public byte DetectRank { get; set; }
}
