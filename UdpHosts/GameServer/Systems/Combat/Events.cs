using GameServer.Entities;

namespace GameServer.Systems.Combat;

public readonly record struct ProjectileHitEvent(
    string ShapeName,
    ulong TargetId,
    int DamageAmount,
    ulong SourceId,
    bool HeadShot,
    bool Crit,
    byte DamageType,
    float DamageMod = -1f,
    bool IsAbilityProjectile = false,
    bool IsSplash = false);

public readonly record struct ImpactHitEvent(
    IEntity Target,
    int HealAmount,
    IEntity Source);

/// <summary>
///     Damage a system applied directly rather than through a projectile impact, e.g. an aptitude chain's
///     InflictDamage.
/// </summary>
public readonly record struct InflictDamageEvent(
    ulong TargetId,
    ulong SourceId,
    int DamageAmount,
    byte DamageType,
    DamageOrigin Origin = DamageOrigin.Ability,
    uint AbilityId = 0,
    uint CommandId = 0);