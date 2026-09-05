using System;

namespace GameServer.Entities.Character;

/// <summary>
/// A projectile that an ability (apt <c>FireProjectileCommand</c>) has queued to fire.
/// The client emits <c>FireWeaponProjectile</c>, so we correlate and consume it to fire the ability projectile (instead of the equipped weapon).
/// </summary>
public struct PendingAbilityProjectile
{
    public uint AmmoType;
    public float Damage;
    public float Range;
    public float Spread;
    public byte BurstCount;
    public ushort Hardpoint;
    public bool AimAtTarget;
    public bool UseHomingTarget;
    public bool UseWeaponDamage;
    public uint ActivationTime;
    public Guid ExecutionId;
    public uint QueueTime;
}
