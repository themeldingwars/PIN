namespace GameServer.Systems.Progression;

/// <summary>
///     The client's own XP reward enum, the byte <c>RewardInfo.RewardType</c> carries. This is not
///     <c>dbcharacter::XPRewardType</c>.
/// </summary>
public enum XpRewardUiType : byte
{
    None = 0,
    Kill = 1,
    Assist = 2,
    AssistCritical = 3,
    Execute = 4,
    Revive = 5,
    Heal = 6,
    Headshot = 7,
    KillStreak = 8,
    EndKillStreak = 9,
    MultipleKill = 10,
    FromEncounter = 11,
    FromMission = 12,
    CmdAddXp = 13,
    CmdResetXp = 14,
    FrameLevelUp = 15,
    MatchFirstKill = 16,
    EndOfMatchDeferred = 17,
    MatchLastKill = 18
}

/// <summary>
///     The state of one battleframe after an XP award or a level change.
/// </summary>
/// <param name="ChassisId">The dbitems::RootItem sdb_id of the frame.</param>
/// <param name="Level">The level the frame is at now.</param>
/// <param name="CurrentXp">XP held towards the next level.</param>
/// <param name="LifetimeXp">XP the frame has earned in total.</param>
/// <param name="LevelsGained">How many levels the frame just advanced, 0 when it did not move.</param>
/// <param name="XpToNextLevel">XP still needed to leave the current level, 0 at the top of the curve.</param>
public readonly record struct ProgressionUpdate(
    uint ChassisId,
    uint Level,
    ulong CurrentXp,
    ulong LifetimeXp,
    uint LevelsGained,
    uint XpToNextLevel);
