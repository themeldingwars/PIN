using System;
using System.Collections.Generic;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.WeaponSim;

internal readonly record struct WeaponContext(
    CharacterEntity Entity,
    CharacterEntity.ActiveWeaponDetails Details,
    WeaponModeState Mode,
    WeaponAmmo Ammo,
    List<WeaponFireEvent> Events);

internal abstract class WeaponFire
{
    // Survives a switch between linked modes, they share the heat
    private uint _heat;
    private uint _heatTime;

    protected WeaponFire(WeaponSim sim) => Sim = sim;

    protected WeaponSim Sim { get; }

    protected uint OverheatEnd { get; private set; }

    public abstract void Process(in WeaponContext ctx, uint time);

    /// <summary>
    ///     A running press or reload doesn't carry over.
    /// </summary>
    public virtual void Switch(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details, bool weaponChanged, bool linkedModes, uint time)
    {
        if (weaponChanged || !linkedModes)
        {
            _heat = 0;
            _heatTime = 0;
            OverheatEnd = 0;
        }
    }

    protected static bool IsM1Chargeup(WeaponTemplateResult weapon) => (weapon.WeaponFlags & 7u) == 1u && weapon.MsChargeUp > 0;

    protected static bool CanOverheat(in WeaponContext ctx) => (ctx.Details.Weapon.WeaponFlags & 7u) == 5u && ctx.Details.WeaponProfile.MsChargeUpMax > 0;

    /// <summary>
    ///     From the main mode's template, the active mode's only when it alone spins up.
    /// </summary>
    protected static (uint Cap, uint Min) ChargeupCaps(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails active)
    {
        var main = entity.GetWeaponDetails(0);
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
        completeTime = time + ctx.Details.Weapon.ReloadTime;
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

        _heat = HeatAt(profile, time) + Math.Max(1u, profile.MsPerBurst);
        _heatTime = time;

        // Only above the max, a shot that lands exactly on it still lets the next one through
        if (_heat <= profile.MsChargeUpMax)
        {
            return false;
        }

        OverheatEnd = time + profile.MsOverchargeDelay;
        Sim.Logger.Debug("[FireSim] Overheated (server={ServerTime} client={ClientTime} heat={Heat}/{Max} until={End} weapon={Weapon})", Sim.CurrentTime, time, _heat, profile.MsChargeUpMax, OverheatEnd, ctx.Details.Weapon.DebugName);
        Sim.ActivateWeaponAbility(ctx.Entity, ctx.Details.Weapon.OverchargeAbility, time);
        return true;
    }

    private static int RoundsInPress(WeaponTemplateResult weapon, WeaponAmmo ammo, out bool fullAuto)
    {
        fullAuto = false;
        byte burstMode = (byte)(weapon.WeaponFlags & 7u);
        if (((WeaponFlags)weapon.WeaponFlags).BoltAction || burstMode == 2 || burstMode == 4)
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
    private uint HeatAt(WeaponProfile profile, uint time)
    {
        uint hold = Math.Max(1u, profile.MsPerBurst) + profile.MsSpreadReturnDelay;
        uint elapsed = time - _heatTime;
        if (_heat == 0 || elapsed <= hold || profile.MsSpreadReturn == 0)
        {
            return _heat;
        }

        ulong cooled = (ulong)profile.MsChargeUpMax * (elapsed - hold) / profile.MsSpreadReturn;
        return cooled >= _heat ? 0 : _heat - (uint)cooled;
    }
}
