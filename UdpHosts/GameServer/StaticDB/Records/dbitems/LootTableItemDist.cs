namespace GameServer.Data.SDB.Records.dbitems;
public record class LootTableItemDist
{
    public uint ItemdropId { get; set; }
    public uint DistId { get; set; }
    public ushort MinQuantity { get; set; }
    public ushort Probability { get; set; }
    public ushort LevelRange { get; set; }
    public ushort MaxQuantity { get; set; }
    public ushort LootTableId { get; set; }
    public byte RollWithQuality { get; set; }
    public byte? AllowScaling { get; set; }
}