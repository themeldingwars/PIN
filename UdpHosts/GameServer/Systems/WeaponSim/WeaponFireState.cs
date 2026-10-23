using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     Weapon fire state machine states
/// </summary>
public enum WeaponFireStateEnum
{
    Idle = 0,
    Firing = 1,
    Reloading = 2,
}

/// <summary>
///     Change-time events that feed the weapon fire state machine.
///     Each carries the client time the event happened at.
/// </summary>
public enum WeaponFireEventType
{
    FireBurst = 0,
    FireEnd = 1,
    FireCancel = 2,
    UseScope = 3,
    SelectWeapon = 4,
    SelectFireMode = 5,
    ReloadWeapon = 6,
    CancelReload = 7,
    FireAimSample = 8,
    FireInputIgnored = 9,
}

/// <summary>
///     A timestamped fire event processed by WeaponSim.Tick.
/// </summary>
public struct WeaponFireEvent
{
    public uint Time;
    public WeaponFireEventType Type;

    // SelectWeapon: weapon index. SelectFireMode: fire mode. UseScope: in-scope flag.
    // FireInputIgnored: the client's ignored flag (non-zero = the input was ignored).
    public byte Value;

    // FireAimSample only
    public Vector3 Aim;
    public Vector3? Velocity;
}

/// <summary>
///     Aim/velocity sample from a client FireWeaponProjectile, consumed by the sim when it fires a shot.
/// </summary>
public struct FireAimSample
{
    public uint Time;
    public Vector3 Aim;
    public Vector3? Velocity;
}

/// <summary>
///     Firing/reload mechanics of the active fire mode.
/// </summary>
public class WeaponFireState
{
    public readonly List<FireAimSample> AimSamples = [];

    public WeaponFireStateEnum State = WeaponFireStateEnum.Idle;

    public uint FireStartTime;
    public uint NextShotTime;

    // -1 = unlimited (full auto while held), 0 = none, >0 = shots left in this press (semi/burst)
    public int ShotsRemainingInBurst;

    // Total rounds in the current press, for spread heat bookkeeping
    public uint TotalRoundsInPress;

    // 0-based index of the next shot within the current press (PRNG round index for burst modes)
    public uint ShotIndexInPress;

    public uint ReloadCompleteTime;

    public uint LatestClientTime;

    public bool TriggerHeld;

    // Client time of the FireBurst that set TriggerHeld; used to (re)start firing with client timing.
    public uint TriggerHeldTime;

    public void Reset()
    {
        State = WeaponFireStateEnum.Idle;
        FireStartTime = 0;
        NextShotTime = 0;
        ShotsRemainingInBurst = 0;
        TotalRoundsInPress = 0;
        ReloadCompleteTime = 0;
        TriggerHeld = false;
        TriggerHeldTime = 0;
        AimSamples.Clear();
    }
}
