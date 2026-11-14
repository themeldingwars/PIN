using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Enums;

namespace GameServer.Systems.Aptitude;

/// <summary>
///     The values LoadRegisterFromStat and StatRequirement read from an entity.
/// </summary>
/// <remarks>
///     Assumption: their stat ids are the <see cref="AptitudeStat" /> ids StatModifier uses. The client caps both at 57 and the
///     Health Powerup's 7 x 0.2 heals the 20 % the replays show.
/// </remarks>
public static class AptitudeStats
{
    /// <summary>
    ///     Null for a stat the server doesn't read yet or an entity without that stat.
    /// </summary>
    public static float? Read(IAptitudeTarget target, ushort stat)
    {
        return (AptitudeStat)stat switch
        {
            AptitudeStat.Health => CurrentHealth(target),
            AptitudeStat.MaxHealth => MaximumHealth(target),
            _ => null,
        };
    }

    private static float? CurrentHealth(IAptitudeTarget target)
    {
        return target switch
        {
            CharacterEntity character => character.CurrentHealth,
            DeployableEntity deployable => deployable.CurrentHealth,
            _ => null,
        };
    }

    private static float? MaximumHealth(IAptitudeTarget target)
    {
        return target switch
        {
            CharacterEntity character => character.MaxHealth.Value,
            DeployableEntity deployable => deployable.MaxHealth,
            _ => null,
        };
    }
}
