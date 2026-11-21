using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbelitelevels;

namespace GameServer.Data;

public class EliteBonuses
{
    public static readonly EliteBonuses None = new();

    public Dictionary<ushort, float> CategoryPercents { get; } = [];

    public Dictionary<ushort, float> Attributes { get; } = [];

    public bool IsEmpty => CategoryPercents.Count == 0 && Attributes.Count == 0;

    public static EliteBonuses From(IEnumerable<BattleframeUpgrade> upgrades, IReadOnlyDictionary<ushort, float> extraBonuses)
    {
        var bonuses = new EliteBonuses();
        foreach (var taken in upgrades ?? [])
        {
            var upgrade = SDBInterface.GetEliteLevelsUpgrade(taken.UpgradeId);
            if (upgrade == null || taken.Count == 0)
            {
                continue;
            }

            var bonus = GetUpgradeBonus(upgrade, taken.Count);
            if (upgrade.AttributeCategory != 0)
            {
                bonuses.Add((ushort)upgrade.AttributeCategory, bonus);
            }
            else if (upgrade.ItemAttribute != 0)
            {
                bonuses.Add((ushort)upgrade.ItemAttribute, bonus);
            }
        }

        foreach (var (id, value) in extraBonuses ?? new Dictionary<ushort, float>())
        {
            bonuses.Add(id, value);
        }

        return bonuses;
    }

    public static float GetUpgradeBonus(EliteLevelsUpgradeTable upgrade, uint count)
    {
        var entries = SDBInterface.GetEliteLevelsTableEntries(upgrade.TableId);
        if (entries.Length == 0)
        {
            return 0f;
        }

        var levels = (int)Math.Min(count, (uint)entries.Length);
        var bonus = entries.Take(levels).Sum();
        if (upgrade.InfiniteLevels != 0 && count > entries.Length)
        {
            bonus += entries[^1] * (count - entries.Length);
        }

        return upgrade.NegativeBoost != 0 ? -bonus : bonus;
    }

    public static uint GetMaxLevel(EliteLevelsUpgradeTable upgrade)
    {
        return upgrade.InfiniteLevels != 0 ? uint.MaxValue : (uint)SDBInterface.GetEliteLevelsTableEntries(upgrade.TableId).Length;
    }

    public static bool IsCategory(uint id) => SDBInterface.GetAttributeCategory(id) != null;

    private void Add(ushort id, float value)
    {
        var target = IsCategory(id) ? CategoryPercents : Attributes;
        target[id] = target.GetValueOrDefault(id) + value;
    }
}
