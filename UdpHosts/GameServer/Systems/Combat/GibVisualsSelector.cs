using System;
using System.Collections.Generic;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbcharacter;

namespace GameServer.Systems.Combat;

/// <summary>
/// Picks the dbcharacter::GibVisuals entry to use for a kill, out of the dbcharacter::HitFeedbackEntry
/// rows keyed by the character's gibset id (that id is the table's hitfeedback_id).
/// </summary>
public static class GibVisualsSelector
{
    // result_type 0 means result_id is a dbcharacter::GibVisuals id, 1 means it is a dbcharacter::Stumble id.
    private const uint ResultTypeGibVisuals = 0;

    // dbcharacter::DamageType id 1 is "Generic". In HitFeedbackEntry it is used as the catch-all damage type; 0 is also
    // treated as unspecified/any.
    private const byte GenericDamageType = 1;

    public static bool TrySelectGibVisualsId(uint gibsetId, DamageInfo killingHit, int maxHealth, int healthAfterHit, out uint gibVisualsId)
    {
        gibVisualsId = 0;

        var entries = SDBInterface.GetHitFeedbackEntries(gibsetId);
        if (entries == null || entries.Count == 0)
        {
            return false;
        }

        float damageHealthFrac = maxHealth > 0 ? (float)killingHit.DamageAmount / maxHealth : 1f;
        float healthFrac = maxHealth > 0 ? (float)Math.Max(0, healthAfterHit) / maxHealth : 0f;

        var candidates = new List<HitFeedbackEntry>();
        foreach (var entry in entries)
        {
            if (entry.ResultType != ResultTypeGibVisuals || entry.ProbWeight == 0)
            {
                continue;
            }

            // Accumulative entries need per-character damage tracking, which does not exist yet.
            if (entry.DamageAccumulated > 0)
            {
                continue;
            }

            if (entry.DamageCrithit != 0 && !killingHit.Crit)
            {
                continue;
            }

            // Generic/0 entry rows match any damage type. Specific rows only match the same damage type, unless the
            // killing hit itself has no damage type.
            bool entryIsGeneric = entry.DamageType == 0 || entry.DamageType == GenericDamageType;
            if (!entryIsGeneric && killingHit.DamageType != 0 && entry.DamageType != killingHit.DamageType)
            {
                continue;
            }

            // health_frac is the health fraction the target has to be at or below, damage_healthfrac the
            // fraction of max health the hit has to deal at least.
            if (entry.HealthFrac < healthFrac || entry.DamageHealthfrac > damageHealthFrac)
            {
                continue;
            }

            candidates.Add(entry);
        }

        return TryPickByWeight(candidates, out gibVisualsId);
    }

    private static bool TryPickByWeight(List<HitFeedbackEntry> candidates, out uint resultId)
    {
        resultId = 0;
        if (candidates.Count == 0)
        {
            return false;
        }

        long totalWeight = 0;
        foreach (var candidate in candidates)
        {
            totalWeight += candidate.ProbWeight;
        }

        long roll = Random.Shared.NextInt64(totalWeight);
        foreach (var candidate in candidates)
        {
            if (roll < candidate.ProbWeight)
            {
                resultId = candidate.ResultId;
                return true;
            }

            roll -= candidate.ProbWeight;
        }

        resultId = candidates[candidates.Count - 1].ResultId;
        return true;
    }
}