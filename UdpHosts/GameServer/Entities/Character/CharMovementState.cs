using System;

namespace GameServer.Entities.Character;

[Flags]
public enum CharMovementState : short
{
    JetSprint = -1 << 14,
    None = 0,
    Crouch = 1 << 0,
    Unknown2 = 1 << 1, // This is only transmitted once by the client during connection, perhaps an "uninitialized" state?
    MovementKeys = 1 << 2,
    Sprint = 1 << 4,
    /* Start Falling */
    MovingTooQuickly1 = 1 << 10,
    MovingTooQuickly2 = 1 << 11,
    /* End Falling */
    TryingToMove = 1 << 12,
    IsMoving = 1 << 13,
    Unk = 1 << 14
}
