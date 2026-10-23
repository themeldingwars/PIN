namespace GameServer.Systems.Loot;

public interface ILootRules
{
    // How long a dropped item stays on the ground.
    int LootLingerMs { get; }

    // Range of the AreaVisualData entity that carries the loot sparkles, and how far a new drop will
    // reuse one instead of spawning another.
    float ContainerRange { get; }

    // Radius around the corpse a drop can land in.
    float DropRadius { get; }

    // How close the collector has to be to a drop.
    float CollectRange { get; }

    // Whether a kill by something that is not player attributable drops anything at all.
    bool RequirePlayerKill { get; }

    // How far above the level of the dead entity a dropped item's RootItem.required_level may be.
    // Negative turns the filter off and lets the shared equipment tables hand out gear of any level.
    int RequiredLevelWindow { get; }
}
