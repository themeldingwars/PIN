namespace GameServer.StaticDB.Records.customdata;

public record SpawnLootCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint LootTableId { get; set; }
}