using System;
using System.Collections.Generic;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.WeaponSim;

public record struct WeaponProfile
{
    public uint WeaponId;

    public float BaseSpreadPct;
    public float OtherSpreadPct;
    public float StartingSpread;
    public float Agility;

    public float SpreadPerBurst;
    public float MinSpreadFrac;
    public float SpreadRampExponent;
    public uint SpreadRampTime;

    public uint MsSpreadReturn;
    public uint MsSpreadReturnDelay;
    public uint MsReturn;
    public uint MsRiseReturnDelay;

    public uint MsAgilityReturn;
    public uint MsAgilityReturnDelay;
    public uint MsPerBurst;

    public float RunMinspreadAdd;
    public float JumpMinspreadAdd;

    public float RunSpreadRampMult;
    public float JumpSpreadRampMult;

    public float ModeFraction;

    public uint MsChargeUp;
    public uint MsChargeUpMin;
    public uint MsChargeUpMax;
    public uint MsOverchargeDelay;

    public ushort MaxAmmo;
    public ushort BaseClipSize;

    public float Range;
    public int DamagePerRound;
    public int MinDamage;
    public float HeadshotMult;
    public uint ReloadTime;
    public uint MsBurstDuration;
    public byte RoundsPerBurst;
    public uint ClipRegenMs;

    public static (WeaponProfile Main, WeaponProfile? Alt) BuildModes(uint weaponId, WeaponTemplateResult main, WeaponTemplateResult alt, Dictionary<ushort, float> attributes)
    {
        var mainProfile = FromTemplate(weaponId, main);
        var altProfile = alt != null ? FromTemplate(weaponId, alt) : mainProfile;
        ApplyAttributes(ref mainProfile, ref altProfile, attributes);
        return (mainProfile, alt != null ? altProfile : null);
    }

    private static WeaponProfile FromTemplate(uint weaponId, WeaponTemplateResult template)
    {
        return new WeaponProfile
        {
            WeaponId = weaponId,

            BaseSpreadPct = template.MinSpread,
            OtherSpreadPct = template.MaxSpread - template.MinSpread,
            StartingSpread = template.StartingSpread,
            Agility = template.Agility,

            SpreadPerBurst = template.SpreadPerBurst,
            MinSpreadFrac = template.MinSpreadFrac,
            SpreadRampExponent = template.SpreadRampExponent,
            SpreadRampTime = template.SpreadRampTime,

            MsSpreadReturn = template.MsSpreadReturn,
            MsSpreadReturnDelay = template.MsSpreadReturnDelay,
            MsReturn = template.MsReturn,
            MsRiseReturnDelay = template.MsRiseReturnDelay,

            MsAgilityReturn = template.MsAgilityReturn,
            MsAgilityReturnDelay = template.MsAgilityReturnDelay,

            MsPerBurst = template.MsPerBurst,

            MsChargeUp = template.MsChargeUp,
            MsChargeUpMin = template.MsChargeUpMin,
            MsChargeUpMax = template.MsChargeUpMax,
            MsOverchargeDelay = template.MsOverchargeDelay,

            RunMinspreadAdd = template.RunMinSpread,
            JumpMinspreadAdd = template.JumpMinSpread,

            RunSpreadRampMult = 1f,
            JumpSpreadRampMult = 1f,

            ModeFraction = 1f,

            BaseClipSize = template.BaseClipSize,
            MaxAmmo = template.MaxAmmo,

            Range = template.Range,
            DamagePerRound = template.DamagePerRound,
            MinDamage = template.MinDamage,
            HeadshotMult = template.HeadshotMult,
            ReloadTime = template.ReloadTime,
            MsBurstDuration = template.MsBurstDuration,
            RoundsPerBurst = template.RoundsPerBurst,
            ClipRegenMs = template.ClipRegenMs,
        };
    }

    private static void ApplyAttributes(ref WeaponProfile main, ref WeaponProfile alt, Dictionary<ushort, float> attributes)
    {
        const float percent = 0.01f;

        if (TryGetAttribute(attributes, ItemAttributeId.RateOfFire, out var rateOfFire) && RoundHalfAway(rateOfFire) is var msPerBurst and not 0)
        {
            uint mainOld = main.MsPerBurst;
            uint altOld = alt.MsPerBurst;
            SetScaled(ref main.MsPerBurst, ref alt.MsPerBurst, (uint)msPerBurst);
            if (mainOld != 0)
            {
                main.MsBurstDuration = (uint)((ulong)main.MsBurstDuration * main.MsPerBurst / mainOld);
            }

            if (altOld != 0)
            {
                alt.MsBurstDuration = (uint)((ulong)alt.MsBurstDuration * alt.MsPerBurst / altOld);
            }
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponReloadSpeed, out var reloadSpeed))
        {
            SetScaled(ref main.ReloadTime, ref alt.ReloadTime, ScaleMs(1, reloadSpeed * 1000f));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponSpread, out var spread))
        {
            ApplySpread(ref main, ref alt, spread);
        }

        if (TryGetAttribute(attributes, ItemAttributeId.Accuracy, out var accuracy))
        {
            main.BaseSpreadPct *= accuracy;
            alt.BaseSpreadPct *= accuracy;
        }

        if (TryGetAttribute(attributes, ItemAttributeId.HeadshotDamage, out var headshot))
        {
            SetScaled(ref main.HeadshotMult, ref alt.HeadshotMult, 1f + (headshot * percent));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponRange, out var range))
        {
            SetScaled(ref main.Range, ref alt.Range, range);
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponChargeup, out var chargeup) || TryGetAttribute(attributes, ItemAttributeId.OverheatTime, out chargeup))
        {
            uint mainOld = main.MsChargeUp;
            uint altOld = alt.MsChargeUp;
            SetScaled(ref main.MsChargeUp, ref alt.MsChargeUp, ScaleMs(1, chargeup * 1000f));
            ShiftChargeupLimits(ref main, (int)(main.MsChargeUp - mainOld));
            ShiftChargeupLimits(ref alt, (int)(alt.MsChargeUp - altOld));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponDamage, out var damage))
        {
            SetScaled(ref main.DamagePerRound, ref alt.DamagePerRound, RoundHalfAway(damage));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.Damage, out var damageMult))
        {
            main.DamagePerRound = (int)(main.DamagePerRound * damageMult);
            alt.DamagePerRound = (int)(alt.DamagePerRound * damageMult);
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponMinimumDamage, out var minDamage))
        {
            SetScaled(ref main.MinDamage, ref alt.MinDamage, RoundHalfAway(minDamage));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.RoundsPerBurst, out var roundsPerBurst) && RoundHalfAway(roundsPerBurst) is var rounds and not 0)
        {
            SetScaled(ref main.RoundsPerBurst, ref alt.RoundsPerBurst, (byte)Math.Clamp(rounds, 0, byte.MaxValue));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.WeaponMagazineSize, out var magazine))
        {
            SetScaled(ref main.BaseClipSize, ref alt.BaseClipSize, (ushort)Math.Clamp(RoundHalfAway(magazine), 1, ushort.MaxValue));
        }

        if (TryGetAttribute(attributes, ItemAttributeId.MaxAmmo, out var maxAmmo))
        {
            main.MaxAmmo = ScaleUShort(main.MaxAmmo, maxAmmo);
            alt.MaxAmmo = ScaleUShort(alt.MaxAmmo, maxAmmo);
        }

        if (TryGetAttribute(attributes, ItemAttributeId.AmmoRegeneration, out var ammoRegen))
        {
            SetScaled(ref main.ClipRegenMs, ref alt.ClipRegenMs, ScaleMs(1000, 1f / ammoRegen));
        }

        if (TryGetBonus(attributes, ItemAttributeId.WeaponChargeupBonus, out var chargeupBonus))
        {
            main.MsChargeUp = ScaleMs(main.MsChargeUp, chargeupBonus);
            alt.MsChargeUp = ScaleMs(alt.MsChargeUp, chargeupBonus);
            main.MsChargeUpMin = ScaleMs(main.MsChargeUpMin, chargeupBonus);
            alt.MsChargeUpMin = ScaleMs(alt.MsChargeUpMin, chargeupBonus);
        }

        if (TryGetBonus(attributes, ItemAttributeId.ReloadTimeBonus, out var reloadBonus))
        {
            main.ReloadTime = ScaleMs(main.ReloadTime, reloadBonus);
            alt.ReloadTime = ScaleMs(alt.ReloadTime, reloadBonus);
        }

        if (TryGetBonus(attributes, ItemAttributeId.ClipSizeBonus, out var clipBonus))
        {
            main.BaseClipSize = ScaleUShort(main.BaseClipSize, clipBonus);
            alt.BaseClipSize = ScaleUShort(alt.BaseClipSize, clipBonus);
        }

        if (TryGetBonus(attributes, ItemAttributeId.AmmoCapacityBonus, out var ammoBonus))
        {
            main.MaxAmmo = ScaleUShort(main.MaxAmmo, ammoBonus);
            alt.MaxAmmo = ScaleUShort(alt.MaxAmmo, ammoBonus);
        }
    }

    private static void ApplySpread(ref WeaponProfile main, ref WeaponProfile alt, float spread)
    {
        const float epsilon = 0.001f;
        float mainMax = main.BaseSpreadPct + main.OtherSpreadPct;
        float altMax = alt.BaseSpreadPct + alt.OtherSpreadPct;
        float newAltMax = mainMax != 0f ? altMax * spread / mainMax : spread;

        if (mainMax > epsilon)
        {
            main.BaseSpreadPct = main.BaseSpreadPct * spread / mainMax;
        }

        if (altMax > epsilon)
        {
            alt.BaseSpreadPct = alt.BaseSpreadPct * newAltMax / altMax;
        }

        main.OtherSpreadPct = spread - main.BaseSpreadPct;
        alt.OtherSpreadPct = newAltMax - alt.BaseSpreadPct;
    }

    private static void ShiftChargeupLimits(ref WeaponProfile profile, int delta)
    {
        if (profile.MsChargeUpMin != 0)
        {
            profile.MsChargeUpMin = (uint)Math.Max(0, (int)profile.MsChargeUpMin + delta);
        }

        if (profile.MsChargeUpMax != 0)
        {
            profile.MsChargeUpMax = (uint)Math.Max(1, (int)profile.MsChargeUpMax + delta);
        }
    }

    private static bool TryGetAttribute(Dictionary<ushort, float> attributes, ItemAttributeId id, out float value)
    {
        const float epsilon = 0.001f;
        return attributes.TryGetValue((ushort)id, out value) && MathF.Abs(value) > epsilon;
    }

    private static bool TryGetBonus(Dictionary<ushort, float> attributes, ItemAttributeId id, out float multiplier)
    {
        if (!TryGetAttribute(attributes, id, out var percent))
        {
            multiplier = 1f;
            return false;
        }

        const float minMultiplier = 0.1f;
        multiplier = Math.Max(minMultiplier, percent * 0.01f);
        return true;
    }

    private static int RoundHalfAway(float value) => (int)(value < 0f ? value - 0.5f : value + 0.5f);

    private static uint ScaleMs(uint value, float multiplier)
    {
        var milli = (ulong)Math.Max(0f, MathF.Floor((multiplier * 1000f) + 0.5f));
        return (uint)Math.Min((ulong)value * milli / 1000, uint.MaxValue);
    }

    private static ushort ScaleUShort(ushort value, float multiplier) => (ushort)Math.Clamp((int)(value * multiplier), 0, ushort.MaxValue);

    private static void SetScaled(ref uint main, ref uint alt, uint value)
    {
        uint old = main;
        main = value;
        alt = old != 0 ? (uint)((ulong)alt * value / old) : value;
    }

    private static void SetScaled(ref int main, ref int alt, int value)
    {
        int old = main;
        main = value;
        alt = old != 0 ? (int)((long)alt * value / old) : value;
    }

    private static void SetScaled(ref ushort main, ref ushort alt, ushort value)
    {
        ushort old = main;
        main = value;
        alt = old != 0 ? (ushort)(alt * value / old) : value;
    }

    private static void SetScaled(ref byte main, ref byte alt, byte value)
    {
        byte old = main;
        main = value;
        alt = old != 0 ? (byte)Math.Min(alt * value / old, byte.MaxValue) : value;
    }

    private static void SetScaled(ref float main, ref float alt, float value)
    {
        float old = main;
        main = value;
        alt = old != 0f ? alt * value / old : value;
    }
}
