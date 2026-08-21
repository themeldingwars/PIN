using System.Numerics;
using AeroMessages.GSS.V66.Character;

namespace GameServer.Systems.MovementRelay;

/// <summary>
///     A single movement sample recorded from the client's MovementInput command.
///     Samples are buffered per character to interpolate/predict the pose between updates.
/// </summary>
public struct MovementSample
{
    public ushort ShortTime;
    public Vector3 Position;
    public Quaternion Orientation;
    public Vector3 Velocity;
    public short MovementState;
    public sbyte HorizontalInput;
    public sbyte VerticalInput;
    public MovementInputFlags InputFlags;

    /// <summary>
    ///     Milliseconds elapsed from one short time to another, allowing overflow.
    ///     Valid while the elapsed time is less than 32768 ms.
    /// </summary>
    /// <param name="from">The earlier short time</param>
    /// <param name="to">The later short time</param>
    /// <returns>The elapsed time in ms between the two short times</returns>
    public static ushort MsSince(ushort from, ushort to) => unchecked((ushort)(to - from));
}
