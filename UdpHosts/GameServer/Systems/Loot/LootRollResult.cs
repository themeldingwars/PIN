namespace GameServer.Systems.Loot;

/// <summary>
/// One item a loot roll produced, already merged per item id.
/// </summary>
public readonly record struct LootRollResult(uint SdbId, uint Quantity);
