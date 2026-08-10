using System;

namespace GameServer.Systems.WeaponSim;

public static class WeaponSpreadMath
{
    private const float _epsilon = 1.1920929e-7f;

    private const float _accuracyCrouch = 0.15f;
    private const float _accuracyMove = 1.25f;
    private const float _accuracyFall = 2.0f;
    private const float _accuracyJetpack = 2.0f;

    public static uint ScaleMs(uint ms, float fraction)
    {
        if (fraction <= 0f)
        {
            return 0;
        }

        return (uint)Math.Round(ms * fraction, MidpointRounding.AwayFromZero);
    }

    // Fast power approximation: lerp between adjacent integer powers.
    public static float ApplyRampExponent(float state, float exponent)
    {
        if (MathF.Abs(exponent) < _epsilon)
        {
            return 1f;
        }

        if (exponent < 0f)
        {
            exponent = -exponent;
            state = 1f - state;
        }

        if (MathF.Abs(exponent - 1f) < _epsilon)
        {
            return state;
        }

        bool complement = false;

        if (exponent < 1f)
        {
            complement = true;
            state = 1f - state;
            exponent = 1f / exponent;
        }

        int power = (int)exponent;
        float frac = exponent - power;

        float current = state;
        float previous = 0f;
        for (int i = 0; i < power; i++)
        {
            previous = current;
            current *= state;
        }

        float result = (current * frac) + ((1f - frac) * previous);
        return complement ? 1f - result : result;
    }

    public static byte GetSpreadState(ushort movementStateValue)
    {
        int high = movementStateValue & 0xf000;

        return high switch
        {
            0x2000 => 0,
            0x3000 or 0x6000 or 0xc000 => 1,
            _ => 2
        };
    }

    public static void UpdateMovementState(WeaponSpreadProfile profile, WeaponModeState state, ushort movementStateValue, uint time)
    {
        float agilityCurrent = AgilitySpreadCheck(profile, state, time);

        state.PreviousMovementFlags = state.CurrentMovementFlags;
        state.CurrentMovementFlags = movementStateValue;

        state.AgilityCurrent = agilityCurrent;
        state.AgilityLastTime = time;
        state.AgilityTarget = GetAgilityTarget(movementStateValue, profile.Agility);

        byte newSpreadState = GetSpreadState(movementStateValue);
        if (newSpreadState != state.SpreadMovementState)
        {
            state.PreviousSpreadFloor = SpreadInterpUpdate(profile, state, time);

            state.OldSpreadMovementState = state.SpreadMovementState;
            state.SpreadMovementState = newSpreadState;
            state.SpreadStateLastTime = time;
        }
    }

    public static float AgilitySpreadCheck(WeaponSpreadProfile profile, WeaponModeState state, uint time)
    {
        uint returnMs = ScaleMs(profile.MsAgilityReturn, profile.ModeFraction);
        uint delayMs = ScaleMs(profile.MsAgilityReturnDelay, profile.ModeFraction);

        float returnAdj = returnMs * profile.Agility;
        float delayAdj = delayMs * profile.Agility;

        float target = state.AgilityTarget;
        float current = state.AgilityCurrent;

        uint timeSince = time >= state.AgilityLastTime ? time - state.AgilityLastTime : 0;

        if (target <= current)
        {
            return target;
        }

        float total = returnAdj + delayAdj;

        if (timeSince >= total)
        {
            return target;
        }

        if (timeSince < delayAdj)
        {
            return current;
        }

        if (returnAdj <= 0f)
        {
            return target;
        }

        float frac = (timeSince - delayAdj) / returnAdj;
        return (frac * target) + ((1f - frac) * current);
    }

    public static float SpreadInterpUpdate(WeaponSpreadProfile profile, WeaponModeState state, uint time)
    {
        uint returnMs = ScaleMs(profile.MsSpreadReturn, profile.ModeFraction);
        uint delayMs = ScaleMs(profile.MsSpreadReturnDelay, profile.ModeFraction);

        return SpreadReturnCheck(profile, state, time, returnMs, delayMs);
    }

    public static void RecoilInterpUpdate(WeaponSpreadProfile profile, WeaponModeState state, uint time, bool snapshot)
    {
        uint timeSince = time >= state.LastRecoilUpdate ? time - state.LastRecoilUpdate : 0;

        uint scaledReturn = ScaleMs(profile.MsReturn, profile.ModeFraction);
        uint scaledSpreadReturn = ScaleMs(profile.MsSpreadReturn, profile.ModeFraction);
        uint scaledRiseDelay = ScaleMs(profile.MsRiseReturnDelay, profile.ModeFraction);
        uint scaledSpreadDelay = ScaleMs(profile.MsSpreadReturnDelay, profile.ModeFraction);

        uint maxReturn = Math.Max(scaledSpreadReturn, scaledReturn);
        uint maxDelay = Math.Max(scaledSpreadDelay, scaledRiseDelay);

        if (MathF.Abs(profile.OtherSpreadPct) <= _epsilon || timeSince >= scaledSpreadReturn + scaledSpreadDelay)
        {
            state.AccumulatedSpread = 0f;
        }
        else if (scaledSpreadDelay < timeSince)
        {
            float frac = scaledSpreadReturn > 0 ? (float)(timeSince - scaledSpreadDelay) / (float)scaledSpreadReturn : 1f;
            state.AccumulatedSpread = (1f - frac) * state.AccumulatedSpreadWhenReturnStarted;
        }
        else
        {
            state.AccumulatedSpread = state.AccumulatedSpreadWhenReturnStarted;
        }

        if (profile.SpreadRampTime > 0 && timeSince < maxDelay + maxReturn)
        {
            if (timeSince < maxDelay)
            {
                state.SpreadHeat = state.SpreadHeatWhenReturn;
            }
            else
            {
                float frac = maxReturn > 0 ? (float)(timeSince - maxDelay) / (float)maxReturn : 1f;
                state.SpreadHeat = (1f - frac) * state.SpreadHeatWhenReturn;
            }
        }
        else
        {
            state.SpreadHeat = 0f;
        }

        if (snapshot)
        {
            state.AccumulatedSpreadWhenReturnStarted = state.AccumulatedSpread;
            state.SpreadHeatWhenReturn = state.SpreadHeat;
            state.LastRecoilUpdate = time;
        }
    }

    public static float GetCurrentSpreadPct(WeaponSpreadProfile profile, WeaponModeState state, uint time)
    {
        float movementPct = SpreadInterpUpdate(profile, state, time);
        float agilityFactor = AgilitySpreadCheck(profile, state, time);

        float y = movementPct + profile.BaseSpreadPct;
        float startingPct = profile.StartingSpread * profile.OtherSpreadPct;
        float z = startingPct + y + state.AccumulatedSpread;

        float upper = profile.OtherSpreadPct + y;

        float clamped = z;
        if (clamped < y)
        {
            clamped = y;
        }

        if (clamped > upper)
        {
            clamped = upper;
        }

        float mult = 1f + (profile.Agility * (agilityFactor - 1f));
        if (mult < 0f)
        {
            mult = 0f;
        }

        return mult * clamped;
    }

    public static void ApplyBurstSpreadUpdate(WeaponSpreadProfile profile, WeaponModeState state, uint firedRounds, uint totalBurstRounds)
    {
        float bulletFraction = totalBurstRounds == 0 ? 1f : firedRounds / (float)totalBurstRounds;

        byte spreadState = GetSpreadState(state.CurrentMovementFlags);

        float heat = 1f;

        if (profile.SpreadRampTime != 0)
        {
            float numerator = profile.MsPerBurst * bulletFraction;
            float denominator = profile.SpreadRampTime;

            float rampMult = spreadState switch
            {
                0 => profile.RunSpreadRampMult,
                1 => profile.JumpSpreadRampMult,
                _ => 1f
            };

            heat = state.SpreadHeatWhenReturn + (rampMult * (numerator / denominator));

            if (heat > 1f)
            {
                heat = 1f;
            }
        }

        state.SpreadHeatWhenReturn = heat;
        state.SpreadHeat = heat;

        float spreadHeat = ApplyRampExponent(state.SpreadHeat, profile.SpreadRampExponent);
        float spreadFactor = (spreadHeat * (1f - profile.MinSpreadFrac)) + profile.MinSpreadFrac;

        float maxAccumulated = profile.OtherSpreadPct * spreadFactor;
        float newAccumulated = (profile.SpreadPerBurst * bulletFraction * spreadFactor) + state.AccumulatedSpreadWhenReturnStarted;

        float maxAbs = MathF.Abs(maxAccumulated);
        if (maxAbs > 0f)
        {
            newAccumulated = Math.Clamp(newAccumulated, -maxAbs, maxAbs);
        }
        else
        {
            newAccumulated = 0f;
        }

        state.AccumulatedSpreadWhenReturnStarted = newAccumulated;
        state.AccumulatedSpread = newAccumulated;
    }

    // Mirrors the client weapon-mode switch handler (fsWeapon::FUN_007b8700).
    // When the active fire mode changes, the client re-seeds the new mode's spread state:
    //   - hasScope (weapon scope data present): A := OtherSpread, heat := 1 (opens at cap, decays over MsSpreadReturn).
    //   - linked modes + other mode has spread: inherit heat = max(self, other) and
    //     A := spreadFactor(heat) * other.A * (newOther / otherOther), after decaying the other mode to `time`.
    //   - otherwise: the new mode simply continues from its own decayed state.
    // The `newState` is the (possibly stale) state of the mode being activated; `otherState` the state of the
    // mode being left (may be fresh/zero when the weapon just changed).
    public static void SeedModeStateOnSwitch(
        WeaponSpreadProfile newProfile,
        WeaponModeState newState,
        WeaponSpreadProfile otherProfile,
        WeaponModeState otherState,
        bool newHasScope,
        bool newLinked,
        bool otherLinked,
        uint time)
    {
        // Common: decay the activating mode's state to `time` and snapshot it.
        RecoilInterpUpdate(newProfile, newState, time, snapshot: true);

        if (newHasScope)
        {
            newState.AccumulatedSpreadWhenReturnStarted = newProfile.OtherSpreadPct;
            newState.SpreadHeat = 1f;
        }
        else if (otherState != null && (newLinked || otherLinked) && otherProfile.OtherSpreadPct > 0f)
        {
            // Decay the other mode to `time` and snapshot it so we inherit its current values.
            RecoilInterpUpdate(otherProfile, otherState, time, snapshot: true);

            newState.SpreadHeat = MathF.Max(newState.SpreadHeatWhenReturn, otherState.SpreadHeatWhenReturn);

            float spreadHeat = ApplyRampExponent(newState.SpreadHeat, newProfile.SpreadRampExponent);
            float spreadFactor = (spreadHeat * (1f - newProfile.MinSpreadFrac)) + newProfile.MinSpreadFrac;

            newState.AccumulatedSpreadWhenReturnStarted =
                spreadFactor * otherState.AccumulatedSpreadWhenReturnStarted * (newProfile.OtherSpreadPct / otherProfile.OtherSpreadPct);
        }

        // Common tail: A := A_whenReturn, heatWhenReturn := heat.
        newState.AccumulatedSpread = newState.AccumulatedSpreadWhenReturnStarted;
        newState.SpreadHeatWhenReturn = newState.SpreadHeat;
    }

    private static float GetAgilityTarget(ushort movementStateValue, float weaponAgility)
    {
        int high = movementStateValue & 0xf000;

        float target = high switch
        {
            0x2000 => ((_accuracyMove - 1f) * weaponAgility) + 1f,
            0x3000 => ((_accuracyFall - 1f) * weaponAgility) + 1f,
            0x6000 or 0xc000 => ((_accuracyJetpack - 1f) * weaponAgility) + 1f,
            _ => 1f
        };

        if ((movementStateValue & 0x0001) != 0)
        {
            target *= _accuracyCrouch;
        }

        return target;
    }

    private static float SpreadReturnCheck(WeaponSpreadProfile profile, WeaponModeState state, uint time, uint returnMs, uint delayMs)
    {
        float stateValue = state.SpreadMovementState switch
        {
            0 => profile.RunMinspreadAdd,
            1 => profile.JumpMinspreadAdd,
            _ => 0f
        };

        uint timeSince = time >= state.SpreadStateLastTime ? time - state.SpreadStateLastTime : 0;

        if (stateValue > state.PreviousSpreadFloor)
        {
            return stateValue;
        }

        if (timeSince >= returnMs + delayMs)
        {
            return stateValue;
        }

        if (timeSince < delayMs)
        {
            return state.PreviousSpreadFloor;
        }

        if (returnMs == 0)
        {
            return stateValue;
        }

        float frac = (timeSince - delayMs) / (float)returnMs;
        return (frac * stateValue) + ((1f - frac) * state.PreviousSpreadFloor);
    }
}
