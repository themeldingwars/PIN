using AeroMessages.GSS;

namespace GameServer.Systems.Combat;

/// <summary>
///
///     An entity's own stance towards each faction.
///     It is two bitfields with one bit per faction.
///     The client expands the pair into one stance byte per faction (fsHostility.cpp): every faction starts out
///     Neutral, the Friendly bitfield sets stance 0 and the Hostile bitfield sets stance 2 afterwards, so a faction
///     in neither bitfield is Neutral and the Hostile bit wins if a faction somehow ends up in both.
/// </summary>
public readonly struct PersonalFactionStance
{
    private readonly PersonalFactionStanceData _data;

    public PersonalFactionStance(PersonalFactionStanceData data)
    {
        _data = data;
    }

    /// <summary>
    ///     The wire form, for pushing the stance back out onto an entity's view.
    /// </summary>
    public PersonalFactionStanceData Data => _data;

    /// <summary>
    ///     This entity's stance towards a faction. Faction ids are 1 based while the bitfields are 0 based, and a
    ///     faction past the end of the bitfield is Neutral.
    /// </summary>
    /// <param name="factionId">The 1-based faction id to look up.</param>
    /// <returns>This entity's stance towards the faction.</returns>
    public HostilityStance GetStance(uint factionId)
    {
        if (factionId == 0)
        {
            return HostilityStance.Friendly;
        }

        if (HasBit(_data.Hostile, factionId))
        {
            return HostilityStance.Hostile;
        }

        return HasBit(_data.Friendly, factionId) ? HostilityStance.Friendly : HostilityStance.Neutral;
    }

    private static bool HasBit(PersonalFactionStanceBitfield bitfield, uint factionId)
    {
        var bit = factionId - 1;
        if (bitfield.Bitfield == null || bit >= bitfield.NumFactions || bit >= bitfield.Bitfield.Length * 8u)
        {
            return false;
        }

        // The client stores these as an array of 64 bit words (slbitset), which on the wire is little endian with
        // the lowest faction in the least significant bit of the first byte.
        return (bitfield.Bitfield[bit / 8] & (1 << (int)(bit % 8))) != 0;
    }
}
