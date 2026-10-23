using System.Collections.Generic;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Progression;

public sealed class StandardProgressionRules : IProgressionRules
{
    private const float _fallbackScale = 1f;

    private readonly List<XpLevelScale> _levelScales;

    public StandardProgressionRules()
        : this(CustomDBInterface.GetXpLevelScales())
    {
    }

    public StandardProgressionRules(List<XpLevelScale> levelScales)
    {
        _levelScales = levelScales ?? [];
    }

    public float XpBoost { get; init; } = 1f;

    public float GetLevelScale(uint level)
    {
        if (_levelScales.Count == 0)
        {
            return _fallbackScale;
        }

        foreach (var entry in _levelScales)
        {
            if (level >= entry.MinLevel && level <= entry.MaxLevel)
            {
                return entry.Scale;
            }
        }

        var closest = _levelScales.Find(entry => level < entry.MinLevel);

        return closest?.Scale ?? _levelScales[^1].Scale;
    }
}
