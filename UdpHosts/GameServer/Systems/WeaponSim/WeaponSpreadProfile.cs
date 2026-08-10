using System;
using GameServer.StaticDB;

namespace GameServer.Systems.WeaponSim;

public record struct WeaponSpreadProfile
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

    // Builds the profile for the active fire mode (Main or Alt template).
    public static WeaponSpreadProfile Build(WeaponTemplateResult mode, uint weaponId, float? mainWeaponSpreadAttribute, float mainMaxSpread, float msPerBurstOverride = 0f)
    {
        float baseSpreadPct;
        float otherSpreadPct;

        if (mainWeaponSpreadAttribute is float attr)
        {
            if (mainMaxSpread > 0f)
            {
                float scale = attr / mainMaxSpread;
                baseSpreadPct = mode.MinSpread * scale;
                otherSpreadPct = (mode.MaxSpread - mode.MinSpread) * scale;
            }
            else
            {
                baseSpreadPct = mode.MinSpread * attr;
                otherSpreadPct = attr - baseSpreadPct;
            }
        }
        else
        {
            baseSpreadPct = mode.MinSpread;
            otherSpreadPct = mode.MaxSpread - mode.MinSpread;
        }

        return new WeaponSpreadProfile
        {
            WeaponId = weaponId,

            BaseSpreadPct = baseSpreadPct,
            OtherSpreadPct = otherSpreadPct,
            StartingSpread = mode.StartingSpread,
            Agility = mode.Agility,

            SpreadPerBurst = mode.SpreadPerBurst,
            MinSpreadFrac = mode.MinSpreadFrac,
            SpreadRampExponent = mode.SpreadRampExponent,
            SpreadRampTime = mode.SpreadRampTime,

            MsSpreadReturn = mode.MsSpreadReturn,
            MsSpreadReturnDelay = mode.MsSpreadReturnDelay,
            MsReturn = mode.MsReturn,
            MsRiseReturnDelay = mode.MsRiseReturnDelay,

            MsAgilityReturn = mode.MsAgilityReturn,
            MsAgilityReturnDelay = mode.MsAgilityReturnDelay,

            // RateOfFire attribute overrides template MsPerBurst when present.
            MsPerBurst = msPerBurstOverride > 0f ? (uint)Math.Round(msPerBurstOverride) : mode.MsPerBurst,

            RunMinspreadAdd = mode.RunMinSpread,
            JumpMinspreadAdd = mode.JumpMinSpread,

            RunSpreadRampMult = 1f,
            JumpSpreadRampMult = 1f,

            ModeFraction = 1f,
        };
    }
}
