using System;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbitems;

namespace GameServer.Data;

public static class ItemLevelScaling
{
    public const uint PowerCategory = 10003;
    public const uint HealthCategory = 10013;

    private const int _tableSize = 80;

    private static float[] _powerTable;
    private static float[] _healthTable;

    private static float[] PowerTable => _powerTable ??= BuildTable("progression.base");

    private static float[] HealthTable => _healthTable ??= BuildTable("progression.health");

    public static float GetLevelSyncFactor(RootItem item, byte effectiveLevel, uint attributeCategory)
    {
        var table = attributeCategory switch
        {
            PowerCategory => PowerTable,
            HealthCategory => HealthTable,
            _ => null,
        };

        if (table == null || item == null || effectiveLevel >= item.RequiredLevel || item.Level == 0)
        {
            return 1f;
        }

        var level = Math.Min((int)item.Level, _tableSize);
        var synced = level - Math.Min(level, item.RequiredLevel - effectiveLevel);
        return table[synced] / table[level];
    }

    public static float GetCharacterScalarSyncFactor(RootItem item, byte effectiveLevel, uint attributeCategory)
    {
        if (attributeCategory != PowerCategory || item == null || effectiveLevel >= item.RequiredLevel || item.Level == 0)
        {
            return 1f;
        }

        var synced = item.Level - Math.Min(item.Level, item.RequiredLevel - effectiveLevel);
        return (float)synced / item.Level;
    }

    public static float ApplyModulePercent(float value, ushort attributeId, uint attributeCategory, float modulePercent, AttributeRange range)
    {
        var category = SDBInterface.GetAttributeCategory(attributeCategory);
        if (category == null)
        {
            return value;
        }

        var percent = modulePercent * (SDBInterface.GetAttributeDefinition(attributeId)?.ModuleEffect ?? 1f);
        var min = 0f;
        var max = float.MaxValue;
        if (range != null)
        {
            percent *= range.ModuleEffect * category.ModuleEffectiveness;
            min = range.ModuleMin;
            max = range.ModuleMax;
        }

        if (category.IsScalar == 0)
        {
            value += percent;
        }
        else if (percent < 0f)
        {
            value /= 1f - (percent / 100f);
        }
        else
        {
            value *= 1f + (percent / 100f);
        }

        return min <= max ? Math.Clamp(value, min, max) : value;
    }

    private static float[] BuildTable(string prefix)
    {
        var mult = SDBInterface.GetConsoleVariableFloat($"{prefix}.ItemLevel.MultGrowth", 1f);
        var linear = SDBInterface.GetConsoleVariableFloat($"{prefix}.ItemLevel.LinearGrowth", 1f);
        var table = new float[_tableSize + 1];
        table[0] = 1f;
        for (var i = 1; i <= _tableSize; i++)
        {
            table[i] = (table[i - 1] * (1f + mult)) + linear;
        }

        return table;
    }
}
