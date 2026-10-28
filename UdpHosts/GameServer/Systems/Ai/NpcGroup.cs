using System.Collections.Generic;
using System.Numerics;
using GameServer.Entities.Character;

namespace GameServer.Systems.Ai;

/// <summary>
///     NPCs that spawned together and count as dead once all of them are. Their <see cref="Rules"/> decide whether they
///     share a home.
/// </summary>
public sealed class NpcGroup(Vector3 home, GroupRules rules)
{
    private readonly Dictionary<ulong, Vector3> _homes = new();

    /// <summary>Where the group spawned.</summary>
    public Vector3 Home { get; } = home;

    public GroupRules Rules { get; } = rules;

    public List<CharacterEntity> Members { get; } = [];

    /// <summary>A member of a group whose members don't share a home keeps <paramref name="home"/> as its own.</summary>
    public void Add(CharacterEntity npc, Vector3 home)
    {
        Members.Add(npc);
        _homes[npc.EntityId] = Rules.SharesHome ? Home : home;
    }

    /// <summary>Where a member belongs, the group's home for one that isn't a member.</summary>
    public Vector3 HomeOf(CharacterEntity npc) => _homes.GetValueOrDefault(npc.EntityId, Home);
}
