namespace GameServer.Systems.Loot;

/// <summary>
/// Conditions a loot table is rolled under. Level is the level of the entity that died - the per-spawn
/// dbcharacter::Monster level, or dbcharacter::Deployable.deployable_level - because the dbitems::LevelBand
/// gates on loot entries are matched against it.
/// </summary>
public sealed class LootRollContext
{
    public int Level { get; init; } = 10;

    // RootItem.class_cert_id of the collector. 0 means unknown, which disables the class_specific filter.
    public uint ClassCertId { get; init; }

    /// <summary>
    /// How far above Level a dropped item's RootItem.required_level is still allowed. The master equipment
    /// tables are shared by every level band and carry no level_range of their own, so without this a level
    /// 1 kill can roll level 60 epic gear. RootItem.level is deliberately not used: it is required_level
    /// plus 5*(quality-1), so gating on it would turn the level filter into a rarity filter.
    /// Negative disables the filter.
    /// </summary>
    public int RequiredLevelWindow { get; init; } = 0;

    /// <summary>
    /// Leaves out items the SDB marks as retired (RootItem.approval_state 4) before anything is picked. The 1962 surface
    /// deposits' tables pick between a retired ore and crystite, and the replays only ever show the crystite.
    /// </summary>
    public bool ExcludeRetired { get; init; }
}
