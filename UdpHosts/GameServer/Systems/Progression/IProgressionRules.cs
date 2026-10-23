namespace GameServer.Systems.Progression;

/// <summary>
/// Tuning knobs of the XP and battleframe progression systems.
/// </summary>
public interface IProgressionRules
{
    /// <summary>
    /// Server wide XP multiplier, applied on top of the XP modifiers the character itself carries.
    /// This is the knob for a global event boost; 1 leaves XP unboosted.
    /// </summary>
    float XpBoost { get; }

    /// <summary>
    /// The multiplier applied to a dbcharacter::XPRewardType base_xp for a kill at the given level.
    /// Retail keeps this curve server side, so it is configured here as level ranges.
    /// </summary>
    /// <param name="level">The level of the entity that died.</param>
    /// <returns>The scaling factor to apply to the reward tier's base_xp.</returns>
    float GetLevelScale(uint level);
}
