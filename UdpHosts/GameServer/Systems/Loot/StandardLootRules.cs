namespace GameServer.Systems.Loot;

public sealed class StandardLootRules : ILootRules
{
    public int LootLingerMs { get; init; } = 90_000;
    public float ContainerRange { get; init; } = 150f;
    public float DropRadius { get; init; } = 3f;
    public float CollectRange { get; init; } = 15f;
    public bool RequirePlayerKill { get; init; } = true;
    public int RequiredLevelWindow { get; init; }
}
