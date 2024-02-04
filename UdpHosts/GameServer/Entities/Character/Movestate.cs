namespace GameServer.Entities.Character;

public enum Movestate : byte
{
    /// <summary>
    /// Standing
    /// </summary>
    Standing = 0x10,

    /// <summary>
    /// Running
    /// </summary>
    Running = 0x20,

    /// <summary>
    /// Falling
    /// </summary>
    Falling = 0x30,

    /// <summary>
    /// Sliding
    /// </summary>
    Sliding = 0x40,

    /// <summary>
    /// Walking
    /// </summary>
    Walking = 0x50,

    /// <summary>
    /// Jetpack
    /// </summary>
    Jetpack = 0x60,

    /// <summary>
    /// Glider
    /// </summary>
    Glider = 0x70,

    /// <summary>
    /// GliderThrusters
    /// </summary>
    GliderThrusters = 0x80,

    /// <summary>
    /// GliderStalling
    /// </summary>
    GliderStalling = 0x90,

    /// <summary>
    /// Knockdown
    /// </summary>
    Knockdown = 0xa0,

    /// <summary>
    /// KnockdownFalling
    /// </summary>
    KnockdownFalling = 0xb0,

    /// <summary>
    /// JetpackSprint
    /// </summary>
    JetpackSprint = 0xc0,

    /// <summary>
    /// Occupant
    /// </summary>
    Occupant = 0xd0,
}
