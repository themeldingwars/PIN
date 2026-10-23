using System.Numerics;

namespace GameServer.Entities.Character;

/// <summary>
///     An aim direction recorded from a client FireWeaponProjectile command.
///     Buffered per character so ability chains can aim with what the client last reported rather than
///     with whatever the live aim happens to be by the time the chain runs.
/// </summary>
public struct AimSample
{
    public ushort ShortTime;
    public Vector3 Aim;

    /// <summary>
    ///     Milliseconds elapsed from one short time to another, allowing overflow.
    ///     Valid while the elapsed time is less than 32768 ms.
    /// </summary>
    /// <param name="from">The earlier short time</param>
    /// <param name="to">The later short time</param>
    /// <returns>The elapsed time in ms between the two short times</returns>
    public static ushort MsSince(ushort from, ushort to) => unchecked((ushort)(to - from));
}