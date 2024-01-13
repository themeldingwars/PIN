namespace GameServer.Data.SDB.Records.dbitems;
public record class LootTable
{
    public float QualityExp { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public ushort MinQuality { get; set; }
    public ushort MaxQuality { get; set; }
    public byte? ClassSpecific { get; set; }
    public byte RollMode { get; set; }
    public byte? AllowQualityScale { get; set; }
    public byte StackDuplicateResults { get; set; }
    public byte? ItemQuality { get; set; }
}
