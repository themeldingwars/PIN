namespace GameServer.Systems.Combat;

/// <summary>
///     How two entities relate to each other. The three real stances keep the same numbers the client uses for
///     its hostility matrix bytes (fshostility.cpp stores 0 = friendly, 1 = neutral, 2 = hostile), so a stance can
///     be handed to the client-side structures without translation.
/// </summary>
public enum HostilityStance : sbyte
{
    Self = -1,

    Friendly = 0,
    Neutral = 1,
    Hostile = 2,
}
