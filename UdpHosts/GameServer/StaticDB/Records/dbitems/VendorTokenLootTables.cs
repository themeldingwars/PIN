namespace GameServer.Data.SDB.Records.dbitems;
public record class VendorTokenLootTables
{
    public float LootTableScale { get; set; }
    public uint LootTableId { get; set; }
    public uint MachineId { get; set; }
    public uint KeyItemId { get; set; }
}
