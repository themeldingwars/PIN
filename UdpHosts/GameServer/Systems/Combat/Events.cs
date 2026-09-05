using GameServer.Entities;

namespace GameServer.Systems.Combat;

public readonly record struct ProjectileHitEvent(
    string ShapeName,
    ulong TargetId,
    int DamageAmount,
    ulong SourceId,
    bool HeadShot,
    bool Crit,
    float DamageMod = -1f,
    bool IsAbilityProjectile = false,
    bool IsSplash = false);

public readonly record struct ImpactHitEvent(
    IEntity Target,
    int HealAmount,
    IEntity Source);