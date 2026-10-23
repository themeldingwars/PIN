namespace GameServer.Data;

public enum CharacterRace : byte
{
    /// <summary>
    /// Default
    /// </summary>
    Human = 0,

    /// <summary>
    /// Deprecated
    /// </summary>
    Falcari = 1,

    /// <summary>
    /// Chosen
    /// </summary>
    DarkOne = 2,

    /// <summary>
    /// Deprecated
    /// </summary>
    Rakshar = 3,

    /// <summary>
    /// Deprecated
    /// </summary>
    Ascendant = 4,

    /// <summary>
    /// Deprecated
    /// </summary>
    Celestial = 5,

    /// <summary>
    /// Standard monster characters
    /// </summary>
    Monster = 6,

    /// <summary>
    /// Friendly monster characters (non-hostile NPCs)
    /// </summary>
    Friendly = 7,

    /// <summary>
    /// Melding-affected monster characters
    /// </summary>
    Melding = 8,

    /// <summary>
    /// Aranha and co
    /// </summary>
    Gaea = 9,

    /// <summary>
    /// Oww you shot me
    /// </summary>
    Bandit = 10,

    /// <summary>
    /// Brontodons?
    /// </summary>
    Neutral = 11,
}