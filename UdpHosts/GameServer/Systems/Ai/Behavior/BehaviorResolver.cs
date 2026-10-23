#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameServer.Systems.Ai.Behavior;

/// <summary>
///     Resolves a raw <see cref="ParsedBehavior" /> into a <see cref="ResolvedBehavior" />: classifies the style from
///     the name and reads the shared parameter blocks (cadence, spacing, ability-moves, perception, wander). Unknown
///     parameter names are ignored. Parsing is invariant-culture and tolerant — an unparseable value leaves the field null.
/// </summary>
public static class BehaviorResolver
{
    public static ResolvedBehavior Resolve(ParsedBehavior behavior)
    {
        var p = behavior.Params;
        var abilities = new List<AbilityMove>(2);
        var am1 = ReadAbility(p, 1);
        if (am1 != null)
        {
            abilities.Add(am1);
        }

        var am2 = ReadAbility(p, 2);
        if (am2 != null)
        {
            abilities.Add(am2);
        }

        var resolved = new ResolvedBehavior(behavior.Name, Classify(behavior.Name), abilities);

        resolved.Cadence.TriggerPullTime = ReadFloat(p, "triggerPullTime");
        resolved.Cadence.FireRestDuration = ReadFloat(p, "fireRestDuration");

        resolved.Spacing.CombatDist = ReadFloat(p, "combatDist");
        resolved.Spacing.MinDist = ReadFloat(p, "minDist");
        resolved.Spacing.PreferredMinCombatDist = ReadFloat(p, "preferredMinCombatDist");
        resolved.Spacing.MaxMove = ReadFloat(p, "maxMove");
        resolved.Spacing.MoveChance = ReadFloat(p, "moveChance");

        resolved.Perception.PerceptionDist = ReadFloat(p, "perceptionDist");
        resolved.Perception.AggroDistance = ReadFloat(p, "aggroDistance");
        resolved.Perception.TargetSelectDist = ReadFloat(p, "targetSelectDist");

        resolved.Wander.NearSpawn = ReadBool(p, "nearSpawn");
        resolved.Wander.Distance = ReadFloat(p, "distance") ?? ReadFloat(p, "wanderDistance");
        resolved.Wander.MoveChance = ReadFloat(p, "moveChance");
        resolved.Wander.PauseDuration = ReadFloat(p, "pauseDuration");

        return resolved;
    }

    /// <summary>
    ///     Classifies a behavior name into a coarse style. Order matters: the specific melee archetypes and the
    ///     wanderer family are matched before the generic ranged/melee buckets (e.g. "GuardCityWanderer" is a wanderer,
    ///     "Arch_AdditiveMelee_Base" is additive melee, not generic melee).
    /// </summary>
    /// <param name="name">The behavior name to classify.</param>
    /// <returns>The coarse behavior style for the name.</returns>
    public static BehaviorStyle Classify(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BehaviorStyle.Unknown;
        }

        if (Contains(name, "AdditiveMelee"))
        {
            return BehaviorStyle.MeleeAdditive;
        }

        if (Contains(name, "FullbodyMelee"))
        {
            return BehaviorStyle.MeleeFullbody;
        }

        if (Contains(name, "Charger"))
        {
            return BehaviorStyle.Charger;
        }

        if (Contains(name, "Kamikaze"))
        {
            return BehaviorStyle.Kamikaze;
        }

        if (Contains(name, "Follower"))
        {
            return BehaviorStyle.Follower;
        }

        if (Contains(name, "Wander"))
        {
            return BehaviorStyle.Wanderer;
        }

        if (Contains(name, "Sniper") || Contains(name, "Ranged") || Contains(name, "MoveThenFire") || Contains(name, "Fire"))
        {
            return BehaviorStyle.Ranged;
        }

        if (Contains(name, "Melee"))
        {
            return BehaviorStyle.Melee;
        }

        if (Contains(name, "Alert") || Contains(name, "Interactive") || Contains(name, "LookAt"))
        {
            return BehaviorStyle.Interaction;
        }

        if (Contains(name, "Stand") || Contains(name, "Stationary") || Contains(name, "Idle"))
        {
            return BehaviorStyle.Idle;
        }

        return BehaviorStyle.Unknown;
    }

    private static bool Contains(string name, string token) => name.Contains(token, StringComparison.Ordinal);

    private static AbilityMove? ReadAbility(Dictionary<string, string> p, int slot)
    {
        var prefix = "am" + slot.ToString(CultureInfo.InvariantCulture);
        var id = ReadInt(p, prefix + "Id");
        var chance = ReadFloat(p, prefix + "Chance");
        var cooldown = ReadFloat(p, prefix + "Cooldown");
        var timeout = ReadFloat(p, prefix + "Timeout");
        var maxDist = ReadFloat(p, prefix + "MaxDist");
        var minDist = ReadFloat(p, prefix + "MinDist");
        var navToDist = ReadFloat(p, prefix + "NavToDist");
        var targeted = ReadBool(p, prefix + "Targeted");
        var facing = ReadBool(p, prefix + "Facing");

        if (id == null && chance == null && cooldown == null && timeout == null && maxDist == null && minDist == null && navToDist == null && targeted == null && facing == null)
        {
            return null;
        }

        return new AbilityMove
        {
            Slot = slot,
            Id = id ?? 0,
            Chance = chance,
            Cooldown = cooldown,
            Timeout = timeout,
            MaxDist = maxDist,
            MinDist = minDist,
            NavToDist = navToDist,
            Targeted = targeted,
            Facing = facing,
        };
    }

    private static float? ReadFloat(Dictionary<string, string> p, string key)
    {
        if (p.TryGetValue(key, out var v) && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
        {
            return f;
        }

        return null;
    }

    private static int? ReadInt(Dictionary<string, string> p, string key)
    {
        if (p.TryGetValue(key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            return i;
        }

        return null;
    }

    private static bool? ReadBool(Dictionary<string, string> p, string key)
    {
        if (!p.TryGetValue(key, out var v))
        {
            return null;
        }

        if (bool.TryParse(v, out var b))
        {
            return b;
        }

        if (v == "1")
        {
            return true;
        }

        if (v == "0")
        {
            return false;
        }

        return null;
    }
}
