namespace GameServer.Data.SDB.Records.dbitems;
public record class LootTableSubTableDist
{
    public uint DistId { get; set; }
    public ushort Probability { get; set; }
    public ushort LevelRange { get; set; }
    public ushort SubtableId { get; set; }
    public ushort LootTableId { get; set; }
    public byte RollWithQuality { get; set; }
    public byte? AllowScaling { get; set; }
}
