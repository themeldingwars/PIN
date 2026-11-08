using System;
using System.Collections.Generic;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     The item attributes the client applies to a weapon's template.
/// </summary>
internal static class WeaponAttributes
{
    /// <summary>
    ///     The character's total plus the weapon's own, like the client sums them. 0 when neither has it.
    /// </summary>
    public static float Total(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details, ItemAttributeId id)
    {
        return (entity.CurrentLoadout?.ItemAttributes.GetValueOrDefault((ushort)id) ?? 0f)
               + details.Attributes.GetValueOrDefault((ushort)id);
    }

    /// <summary>
    ///     Applies a bonus given in percent, at least 10 % and rounded to 1/1000 like in the client. A total of 0 leaves the
    ///     value alone.
    /// </summary>
    public static uint Percent(uint value, float total)
    {
        if (Math.Abs(total) <= 0.001f)
        {
            return value;
        }

        ulong permille = (ulong)Math.Floor((Math.Max(total * 0.01f, 0.1f) * 1000f) + 0.5f);
        return (uint)(value * permille / 1000);
    }

    public static ushort ClipSize(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details)
    {
        return (ushort)Math.Min(ushort.MaxValue, Percent(details.WeaponProfile.BaseClipSize, Total(entity, details, ItemAttributeId.ClipSizeBonus)));
    }

    // Includes the clip
    public static ushort MaxAmmo(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details)
    {
        uint max = details.WeaponProfile.MaxAmmo;
        float factor = Total(entity, details, ItemAttributeId.MaxAmmo);
        if (Math.Abs(factor) > 0.001f)
        {
            max = (uint)Math.Max(0f, max * factor);
        }

        return (ushort)Math.Min(ushort.MaxValue, Percent(max, Total(entity, details, ItemAttributeId.AmmoCapacityBonus)));
    }

    public static uint ReloadTime(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details)
    {
        float seconds = Total(entity, details, ItemAttributeId.WeaponReloadSpeed);
        uint reload = seconds > 0.001f ? (uint)Math.Round(seconds * 1000f) : details.Weapon.ReloadTime;
        return Percent(reload, Total(entity, details, ItemAttributeId.ReloadTimeBonus));
    }

    // One round per interval in ms, 0 when the weapon doesn't regenerate
    public static uint RegenInterval(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details)
    {
        if (details.Weapon.ClipRegenMs == 0)
        {
            return 0;
        }

        float rate = Total(entity, details, ItemAttributeId.AmmoRegeneration);
        return Math.Abs(rate) > 0.001f ? (uint)Math.Max(1, Math.Round(1000f / rate)) : details.Weapon.ClipRegenMs;
    }
}
