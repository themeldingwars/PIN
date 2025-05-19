using System;

namespace GameServer.Entities.Character;

[Flags]
public enum MovementFlags : byte
{
    /// <summary>
    /// Crouch modifier is active
    /// </summary>
    Crouch = 1 << 0,

    /// <summary>
    /// Some movement input is active
    /// </summary>
    Movement = 1 << 2,

    /// <summary>
    /// Sprint modifier is active
    /// </summary>
    Sprint = 1 << 4
}