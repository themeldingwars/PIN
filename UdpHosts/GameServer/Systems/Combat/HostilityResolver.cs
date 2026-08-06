using AeroMessages.GSS.V66;
using GameServer.Entities;
using Serilog;

namespace GameServer.Systems.Combat;

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
        return GetStance(attacker.HostilityInfo, target.HostilityInfo) == HostilityStance.Hostile;
    }

    public HostilityStance GetStance(HostilityInfoData aInfo, HostilityInfoData bInfo)
    {
        // Duel overrides
        if (IsDueling(aInfo, bInfo))
        {
            return HostilityStance.Hostile;
        }

        // Default faction relationship
        return _factionHostility.GetFactionStance(aInfo.FactionId, bInfo.FactionId);
    }

    public bool IsDueling(HostilityInfoData a, HostilityInfoData b)
    {
        return false;
    }
}