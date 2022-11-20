using System;

namespace GameServer.Entities.Character;

[Flags]
public enum CharMovementState : short
{
    None,
    Crouch = 1 << 0,
    Unk2 = 1 << 1, // This is only transmitted once by the client during connection, perhaps an "uninitialized" state?
    MovementKeys = 1 << 2,
    Sprint = 1 << 4,
    TryingToMove = 1 << 12,
    IsMoving = 1 << 13,
    Unk = 1 << 14
}