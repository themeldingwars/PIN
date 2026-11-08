using System;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     Main and Alt are the slot's two modes, Mode is the active one of them.
/// </summary>
internal readonly record struct WeaponContext(
    CharacterEntity Entity,
    byte Slot,
    CharacterEntity.ActiveWeaponDetails Details,
    WeaponModeState Mode,
    WeaponModeState Main,
    WeaponModeState Alt,
    WeaponAmmo Ammo,
    uint SwapReady);

internal abstract class WeaponFire
{
    protected WeaponFire(WeaponSim sim) => Sim = sim;

    protected WeaponSim Sim { get; }

    /// <summary>
    ///     Runs what's due up to the time, the sim calls it before every event and at the end of a tick.
    /// </summary>
    public abstract void AdvanceTo(in WeaponContext ctx, uint time);

    /// <summary>
    ///     Weapon and mode switches never get here, the sim calls <see cref="Switch" /> for them.
    /// </summary>
    public abstract void Handle(in WeaponContext ctx, WeaponFireEvent evt);

    /// <summary>
    ///     A running press or reload doesn't carry over, the heat and the last shot stay with their mode.
    /// </summary>
    public abstract void Switch(in WeaponContext ctx, bool weaponChanged, bool linkedModes, uint time);

    /// <summary>
    ///     Whether heat and spin-up carry across a mode switch, the client's IsLinkedSameMode.
    /// </summary>
    protected static bool LinkedSameMode(CharacterEntity entity, byte slot)
    {
        if (!entity.HasAltMode(slot) || entity.GetWeaponDetails(slot, 0)?.Weapon is not { } main || entity.GetWeaponDetails(slot, 1)?.Weapon is not { } alt)
        {
            return false;
        }

        return (main.Flags.LinkedModes || alt.Flags.LinkedModes) && main.Flags.Mode == alt.Flags.Mode;
    }

    /// <summary>
    ///     Like the client, a burst of one linked mode also holds back the other one.
    /// </summary>
    protected static void RecordShot(in WeaponContext ctx, uint time)
    {
        ctx.Mode.LastShotTime = time;
        var main = ctx.Entity.GetWeaponDetails(ctx.Slot, 0)?.Weapon;
        var alt = ctx.Entity.GetWeaponDetails(ctx.Slot, 1)?.Weapon;
        if (ctx.Entity.HasAltMode(ctx.Slot) && main != null && alt != null && (main.Flags.LinkedModes || alt.Flags.LinkedModes))
        {
            var other = ReferenceEquals(ctx.Mode, ctx.Main) ? ctx.Alt : ctx.Main;
            other.LastShotTime = Math.Max(other.LastShotTime, time);
        }
    }

    protected static uint OverheatEnd(in WeaponContext ctx) => HeatState(ctx).OverheatEnd;

    protected static bool IsM1Chargeup(WeaponTemplateResult weapon) => weapon.Flags.Mode == WeaponFlags.BurstMode.Chargeup && weapon.MsChargeUp > 0;

    protected static bool CanOverheat(in WeaponContext ctx) => ctx.Details.Weapon.Flags.Mode == WeaponFlags.BurstMode.Overheat && ctx.Details.WeaponProfile.MsChargeUpMax > 0;

    /// <summary>
    ///     From the main mode's template, the active mode's only when it alone spins up.
    /// </summary>
    protected static (uint Cap, uint Min) ChargeupCaps(in WeaponContext ctx)
    {
        var active = ctx.Details;
        var main = ctx.Entity.GetWeaponDetails(ctx.Slot, 0);
        WeaponProfile source;
        if (main?.Weapon != null && main.WeaponProfile.MsChargeUp > 0)
        {
            source = main.WeaponProfile;
        }
        else if (active.WeaponProfile.MsChargeUp > 0 || main?.Weapon == null)
        {
            source = active.WeaponProfile;
        }
        else
        {
            source = main.WeaponProfile;
        }

        return (source.MsChargeUp, source.MsChargeUpMin);
    }

    /// <summary>
    ///     Rounds the shot at that index of the press takes, scaled by AmmoConsumption. A burst with a duration splits its
    ///     ammo between its shots, the first ones take the remainder.
    /// </summary>
    protected static int ShotCost(in WeaponContext ctx, uint shotInPress)
    {
        var weapon = ctx.Details.Weapon;
        int ammo = BurstCost(ctx);
        if (!IsTimedBurst(weapon))
        {
            return ammo;
        }

        int index = (int)(shotInPress % weapon.RoundsPerBurst);
        return (ammo / weapon.RoundsPerBurst) + (index < ammo % weapon.RoundsPerBurst ? 1 : 0);
    }

    /// <summary>
    ///     A whole burst's rounds, scaled by AmmoConsumption like in the client. 0 for a heal beam or a spin-up alt.
    /// </summary>
    protected static int BurstCost(in WeaponContext ctx)
    {
        int ammo = (int)ctx.Details.Weapon.AmmoPerBurst;
        float consumption = ctx.Entity.Character_CombatController?.AmmoConsumptionProp.Value ?? 1f;
        return Math.Abs(consumption - 1f) > 0.001f ? (int)(ammo * consumption) : ammo;
    }

    /// <summary>
    ///     Time the shot at that index of the press comes after the previous one, in ms.
    /// </summary>
    protected static uint ShotSpacing(in WeaponContext ctx, uint shotInPress)
    {
        var weapon = ctx.Details.Weapon;
        uint msPerBurst = Math.Max(1u, ctx.Details.WeaponProfile.MsPerBurst);
        if (!IsTimedBurst(weapon))
        {
            return msPerBurst;
        }

        // The rounds of a burst are spread over its duration, the next burst starts MsPerBurst after this one did
        uint between = weapon.MsBurstDuration / (uint)(weapon.RoundsPerBurst - 1);
        return shotInPress % weapon.RoundsPerBurst == 0 ? Math.Max(1u, msPerBurst - weapon.MsBurstDuration) : Math.Max(1u, between);
    }

    /// <summary>
    ///     Returns the rounds of the press, the burst ability needs at least MinRoundsPerBurst of them.
    /// </summary>
    protected int StartPress(in WeaponContext ctx, uint time, out bool fullAuto)
    {
        var weapon = ctx.Details.Weapon;
        int rounds = RoundsInPress(weapon, ctx.Ammo, out fullAuto);
        if (weapon.BurstAbility != 0 && rounds >= Math.Max(1, (int)weapon.MinRoundsPerBurst))
        {
            Sim.ActivateWeaponAbility(ctx.Entity, weapon.BurstAbility, time);
        }

        return rounds;
    }

    /// <summary>
    ///     False with a full clip or an empty reserve.
    /// </summary>
    protected bool StartReload(in WeaponContext ctx, uint time, out uint completeTime)
    {
        // After a shot the reload can't start before the penalty is over
        var weapon = ctx.Details.Weapon;
        uint start = ctx.Ammo.LastShotTime == 0 ? time : Math.Max(time, ctx.Ammo.LastShotTime + weapon.ReloadPenalty + weapon.MsBurstDuration);
        completeTime = start + WeaponAttributes.ReloadTime(ctx.Entity, ctx.Details);
        if (!ctx.Ammo.CanReload)
        {
            return false;
        }

        Sim.ActivateWeaponAbility(ctx.Entity, ctx.Details.Weapon.ReloadAbility, time);
        return true;
    }

    protected void ClipEmpty(in WeaponContext ctx, uint time)
    {
        ctx.Ammo.MarkEmpty();
        Sim.Logger.Debug("[FireSim] Clip empty (server={ServerTime} client={ClientTime} reserve={Reserve} weapon={Weapon})", Sim.CurrentTime, time, ctx.Ammo.Reserve, ctx.Details.Weapon.DebugName);
        Sim.ActivateWeaponAbility(ctx.Entity, ctx.Details.Weapon.EmptyAbility, time);
    }

    /// <summary>
    ///     True when the shot overheats the weapon, it's locked until OverheatEnd then.
    /// </summary>
    protected bool AddHeat(in WeaponContext ctx, uint time)
    {
        var profile = ctx.Details.WeaponProfile;
        if (!CanOverheat(ctx))
        {
            return false;
        }

        var state = HeatState(ctx);
        state.Heat = HeatAt(profile, state, time) + Math.Max(1u, profile.MsPerBurst);
        state.HeatTime = time;

        // Only above the max, a shot that lands exactly on it still lets the next one through
        if (state.Heat <= profile.MsChargeUpMax)
        {
            return false;
        }

        state.OverheatEnd = time + profile.MsOverchargeDelay;
        Sim.Logger.Debug("[FireSim] Overheated (server={ServerTime} client={ClientTime} heat={Heat}/{Max} until={End} weapon={Weapon})", Sim.CurrentTime, time, state.Heat, profile.MsChargeUpMax, state.OverheatEnd, ctx.Details.Weapon.DebugName);
        Sim.ActivateWeaponAbility(ctx.Entity, ctx.Details.Weapon.OverchargeAbility, time);
        return true;
    }

    // The client fires such a burst as one shot per round
    private static bool IsTimedBurst(WeaponTemplateResult weapon) => weapon.MsBurstDuration > 0 && weapon.RoundsPerBurst >= 2;

    private static int RoundsInPress(WeaponTemplateResult weapon, WeaponAmmo ammo, out bool fullAuto)
    {
        fullAuto = false;
        var burstMode = weapon.Flags.Mode;
        if (weapon.Flags.BoltAction || burstMode == WeaponFlags.BurstMode.M2 || burstMode == WeaponFlags.BurstMode.Hold)
        {
            return 1;
        }

        if (weapon.RoundsPerBurst > 1)
        {
            return Math.Max(1, Math.Min(weapon.RoundsPerBurst, ammo.HasAmmo ? ammo.Clip : int.MaxValue));
        }

        fullAuto = true;
        return ammo.HasAmmo ? Math.Max(1, ammo.Clip) : 1;
    }

    // The client cools the heat with the spread return fields, MsChargeUpMax per MsSpreadReturn
    private static uint HeatAt(WeaponProfile profile, WeaponModeState state, uint time)
    {
        uint hold = Math.Max(1u, profile.MsPerBurst) + profile.MsSpreadReturnDelay;
        uint elapsed = time - state.HeatTime;
        if (state.Heat == 0 || elapsed <= hold || profile.MsSpreadReturn == 0)
        {
            return state.Heat;
        }

        ulong cooled = (ulong)profile.MsChargeUpMax * (elapsed - hold) / profile.MsSpreadReturn;
        return cooled >= state.Heat ? 0 : state.Heat - (uint)cooled;
    }

    private static WeaponModeState HeatState(in WeaponContext ctx) => LinkedSameMode(ctx.Entity, ctx.Slot) ? ctx.Main : ctx.Mode;
}
