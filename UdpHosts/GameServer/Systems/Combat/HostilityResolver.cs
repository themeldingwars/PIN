using AeroMessages.GSS;
using GameServer.Entities;
using Serilog;

namespace GameServer.Systems.Combat;

/// <summary>
///     Decides whether two entities may hurt each other. Hostility comes out of three things, in the order the
///     client's fsHostility code applies them: the faction pair (dbcharacter::Faction + FactionRelations), the
///     entities' own PersonalFactionStance, and whatever temporary PvP state rides on their hostility info.
/// </summary>
public class HostilityResolver
{
    private readonly ILogger _logger;
    private readonly FactionHostility _factionHostility;

    public HostilityResolver()
    {
        _logger = Log.ForContext<HostilityResolver>();
        _factionHostility = new FactionHostility();
    }

    public bool CanDamage(IEntity attacker, IEntity target)
    {
        return GetStance(attacker, target) is HostilityStance.Hostile or HostilityStance.Neutral;
    }

    public HostilityStance GetStance(IEntity attacker, IEntity target)
    {
        if (attacker == null || target == null)
        {
            return HostilityStance.Neutral;
        }

        if (attacker.EntityId == target.EntityId)
        {
            return HostilityStance.Self;
        }

        var stance = GetFactionStance(attacker, target);

        if (IsDueling(attacker.HostilityInfo, target.HostilityInfo) ||
            (IsPvpFlagged(attacker.HostilityInfo) && IsPvpFlagged(target.HostilityInfo)))
        {
            stance = HostilityStance.Hostile;
        }

        _logger.Verbose(
            "Hostility {Attacker} ({AttackerFaction}) vs {Target} ({TargetFaction}): {Stance}",
            attacker.EntityId,
            attacker.HostilityInfo.FactionId,
            target.EntityId,
            target.HostilityInfo.FactionId,
            stance);

        return stance;
    }

    public bool IsDueling(HostilityInfoData attackerInfo, HostilityInfoData targetInfo)
    {
        var attackerTeam = attackerInfo.Unk2;
        var attackerOpponent = attackerInfo.Unk3;
        var targetTeam = targetInfo.Unk2;
        var targetOpponent = targetInfo.Unk3;

        if (attackerTeam == 0 || targetTeam == 0)
        {
            return false;
        }

        return attackerTeam == targetOpponent && attackerOpponent == targetTeam;
    }

    private static bool IsPvpFlagged(HostilityInfoData info)
    {
        return (info.Unk4 & 1) != 0;
    }

    private HostilityStance GetFactionStance(IEntity attacker, IEntity target)
    {
        var attackerInfo = attacker.HostilityInfo;
        var targetInfo = target.HostilityInfo;

        var factionStance = _factionHostility.GetFactionStance(attackerInfo.FactionId, targetInfo.FactionId);

        var attackerPersonal = attacker.PersonalFactionStance;
        var targetPersonal = target.PersonalFactionStance;
        if (attackerPersonal.HasValue == targetPersonal.HasValue)
        {
            return factionStance;
        }

        // A side without a faction never reaches the personal stance: the client answers those pairs straight out
        // of the matrix short circuit.
        if (attackerInfo.FactionId == 0 || targetInfo.FactionId == 0)
        {
            return factionStance;
        }

        if (attackerPersonal.HasValue)
        {
            return attackerPersonal.Value.GetStance(targetInfo.FactionId);
        }

        return targetPersonal.Value.GetStance(attackerInfo.FactionId);
    }
}
