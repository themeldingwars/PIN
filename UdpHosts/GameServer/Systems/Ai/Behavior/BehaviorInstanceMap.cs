#nullable enable
using System;
using System.Collections.Generic;

namespace GameServer.Systems.Ai.Behavior;

/// <summary>
///     Explicit mapping from the Monster table's opaque <c>*_instance_id</c> values to behavior definitions.
///     Based on family + weapon-profile analysis.
///
///     Precedence: a real (non-sentinel) text behavior always wins; the instance id is used only when the text column
///     is empty or is a sentinel (<c>_inst</c> / <c>_instattack</c> / <c>_instdefend</c>).
///     Mapped values reuse the same <c>Name(key=value,...)</c> format the text columns use, so they flow through the
///     existing parser/resolver. Values below are initial guesses keyed to each id's dominant family + weapon profile.
/// </summary>
public static class BehaviorInstanceMap
{
    // Behavior templates keyed to the dominant family + weapon profile of each instance id.
    private const string _rangedHumanoid = "Arch_MedRangedHumanoid_Base(triggerPullTime=1500,fireRestDuration=1000,combatDist=20)";
    private const string _rangedHeavy = "Arch_MedRangedHumanoid_Base(triggerPullTime=2000,fireRestDuration=1500,combatDist=25)";
    private const string _sniper = "Arch_Sniper_Base(triggerPullTime=2500,fireRestDuration=3000,combatDist=40)";
    private const string _rangedCreature = "Arch_MedRanged_Base(triggerPullTime=1500,fireRestDuration=1000,combatDist=15)";
    private const string _meleeCreature = "Arch_FullbodyMelee_Base(combatDist=3)";
    private const string _meleeHumanoid = "Arch_AdditiveMelee_Base(combatDist=1)";
    private const string _idle = "Stand";

    private static readonly Dictionary<uint, string> Definitions = new()
    {
        // Human / Chosen assault-rifle grunts (ranged).
        [274] = _rangedHumanoid,
        [262] = _rangedHumanoid,
        [299] = _rangedHumanoid,
        [190] = _rangedHumanoid,
        [148] = _rangedHumanoid,
        [260] = _rangedHumanoid,
        [408] = _rangedHumanoid,
        [146] = _rangedHumanoid,
        [301] = _rangedHumanoid,
        [416] = _rangedHumanoid,
        [415] = _rangedHumanoid,
        [417] = _rangedHumanoid,
        [144] = _rangedHumanoid,
        [359] = _rangedHumanoid,
        [447] = _rangedHumanoid,
        [449] = _rangedHumanoid,
        [250] = _rangedHumanoid, // Chosen NPC, no weapon resolved -> assume ranged combatant (refine later)

        // Shielded / heavy (HMG) humanoids.
        [147] = _rangedHeavy,

        // Sniper rifles.
        [358] = _sniper,

        // Ranged creatures / spitters (creature medium, giant spider, mosquito, aranha spit, hovering).
        [119] = _rangedCreature,
        [334] = _rangedCreature,
        [335] = _rangedCreature,
        [141] = _rangedCreature,
        [128] = _rangedCreature,
        [317] = _rangedCreature,
        [318] = _rangedCreature,
        [377] = _rangedCreature,

        // Melee creatures (aranha, feral canine, giant beetle).
        [140] = _meleeCreature,
        [142] = _meleeCreature,
        [196] = _meleeCreature,
        [118] = _meleeCreature,
        [404] = _meleeCreature,

        // Melee humanoids (Chosen melee).
        [472] = _meleeHumanoid,

        // Pets / companion / drones / civilians (idle).
        [224] = _idle,
        [225] = _idle,
        [226] = _idle,
        [131] = _idle,
        [543] = _idle,
        [537] = _idle,
        [542] = _idle,
    };

    // Text placeholders that mean "the behavior comes from the instance id" rather than being a real behavior.
    private static readonly HashSet<string> _sentinels = new(StringComparer.OrdinalIgnoreCase)
    {
        "_inst",
        "_instattack",
        "_instdefend",
    };

    public static bool IsSentinel(string name) => _sentinels.Contains(name);

    public static bool TryGetDefinition(uint instanceId, out string definition) => Definitions.TryGetValue(instanceId, out definition!);

    /// <summary>
    ///     Resolves one behavior slot: the text column wins unless it is empty or a sentinel, in which case the
    ///     instance id's mapped definition is used. Returns an empty <see cref="ParsedBehavior" /> when neither applies.
    /// </summary>
    /// <param name="text">The behavior text column, or null.</param>
    /// <param name="instanceId">The behavior instance id whose mapped definition is the fallback.</param>
    /// <returns>The resolved behavior, or an empty <see cref="ParsedBehavior" /> when neither source applies.</returns>
    public static ParsedBehavior Resolve(string? text, uint instanceId)
    {
        var parsed = BehaviorParser.Parse(text);
        if (!parsed.IsEmpty && !IsSentinel(parsed.Name))
        {
            return parsed;
        }

        if (instanceId != 0 && Definitions.TryGetValue(instanceId, out var definition))
        {
            return BehaviorParser.Parse(definition);
        }

        // Empty text, or a sentinel with no mapping: treat the slot as absent (unknown instances stay inert).
        return ParsedBehavior.Empty;
    }
}
