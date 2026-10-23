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

    // Builds the profile for a fire mode
    public static WeaponProfile Build(uint weaponId, WeaponTemplateResult template, Dictionary<ushort, float> attributes, float mainMaxSpread)
    {
        float? weaponAttributeSpread = attributes.TryGetValue((ushort)ItemAttributeId.WeaponSpread, out var spreadAttr) ? spreadAttr : null;
        float msPerBurstOverride = attributes.TryGetValue((ushort)ItemAttributeId.RateOfFire, out var rofAttr) ? rofAttr : 0f;
        float weaponMagazineSize = attributes.TryGetValue((ushort)ItemAttributeId.WeaponMagazineSize, out var magAttr) ? magAttr : 0f;

        float baseSpreadPct;
        float otherSpreadPct;

        if (weaponAttributeSpread is float attr)
        {
            if (mainMaxSpread > 0f)
            {
                float scale = attr / mainMaxSpread;
                baseSpreadPct = template.MinSpread * scale;
                otherSpreadPct = (template.MaxSpread - template.MinSpread) * scale;
            }
            else
            {
                baseSpreadPct = template.MinSpread * attr;
                otherSpreadPct = attr - baseSpreadPct;
            }
        }
        else
        {
            baseSpreadPct = template.MinSpread;
            otherSpreadPct = template.MaxSpread - template.MinSpread;
        }

        return new WeaponProfile
        {
            WeaponId = weaponId,

            BaseSpreadPct = baseSpreadPct,
            OtherSpreadPct = otherSpreadPct,
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

            MsPerBurst = msPerBurstOverride > 0f ? (uint)Math.Round(msPerBurstOverride) : template.MsPerBurst,

            MsChargeUp = template.MsChargeUp,
            MsChargeUpMin = template.MsChargeUpMin,
            MsChargeUpMax = template.MsChargeUpMax,
            MsOverchargeDelay = template.MsOverchargeDelay,

            RunMinspreadAdd = template.RunMinSpread,
            JumpMinspreadAdd = template.JumpMinSpread,

            RunSpreadRampMult = 1f,
            JumpSpreadRampMult = 1f,

            ModeFraction = 1f,

            BaseClipSize = weaponMagazineSize > 0f ? (ushort)weaponMagazineSize : template.BaseClipSize,
            MaxAmmo = template.MaxAmmo,
        };
    }
}
