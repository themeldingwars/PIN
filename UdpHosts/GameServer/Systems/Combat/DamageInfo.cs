namespace GameServer.Systems.Combat;

/// <summary>
/// Describes a single applied damage instance, used to resolve dbcharacter::HitFeedbackEntry results.
/// </summary>
public readonly record struct DamageInfo(
    int DamageAmount,
    byte DamageType,
    bool Crit,
    DamageOrigin Origin = DamageOrigin.Weapon);