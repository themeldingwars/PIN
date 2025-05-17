namespace GameServer.Data.SDB.Records.customdata;

public record SpawnLootCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint LootTableId { get; set; }
}
