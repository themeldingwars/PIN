using System.Numerics;

namespace GameServer.Systems.WeaponSim;

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
    Shot = 8,
    FireInputIgnored = 9,
}

/// <summary>
///     Time is the client's, the queue is sorted by it.
/// </summary>
public struct WeaponFireEvent
{
    public uint Time;
    public WeaponFireEventType Type;

    // Weapon index, fire mode, in-scope flag or the client's ignored flag, depending on the type
    public byte Value;

    // SelectFireMode only, the weapon slot whose mode changes
    public byte Slot;

    // Shot only, the client's aim and velocity
    public Vector3 Aim;
    public Vector3? Velocity;
}
