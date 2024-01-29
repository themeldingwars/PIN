using System;
using System.Linq;

namespace GameServer.Entities.Character;

/// <summary>
///     Allows eased access to the several flags
/// </summary>
internal class MovementStateContainer
{
    public ushort MovementStateValue;
    public MovementFlags Flags => (MovementFlags)(MovementStateValue & 0x00FF);
    public Movestate Movestate => (Movestate)((MovementStateValue & 0xF000) >> 8);

    /// <summary>
    ///     Corresponds to the <see cref="MovementFlags.Crouch" /> flag
    /// </summary>
    public bool Crouch
    {
        get => Flags.HasFlag(MovementFlags.Crouch);
        set => SetMovementFlag(MovementFlags.Crouch, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="MovementFlags.Movement" /> flag
    /// </summary>
    public bool Movement
    {
        get => Flags.HasFlag(MovementFlags.Movement);
        set => SetMovementFlag(MovementFlags.Movement, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="MovementFlags.Sprint" /> flag
    /// </summary>
    public bool Sprint
    {
        get => Flags.HasFlag(MovementFlags.Sprint);
        set => SetMovementFlag(MovementFlags.Sprint, value);
    }

    private void SetMovementFlag(MovementFlags flag, bool value)
    {
        if (value)
        {
            MovementStateValue |= (ushort)flag;
        }
        else
        {
            MovementStateValue ^= (ushort)flag;
        }
    }
}