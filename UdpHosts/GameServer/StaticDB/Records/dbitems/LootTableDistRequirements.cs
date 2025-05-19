namespace GameServer.Data.SDB.Records.dbitems;
public record class LootTableDistRequirements
{
    public uint Type { get; set; }
    public uint LootTableDistId { get; set; }
    public float Value1 { get; set; }
    public float Value2 { get; set; }
}