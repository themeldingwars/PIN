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
    protected WeaponFire(WeaponSim sim) => Sim = sim;

    protected WeaponSim Sim { get; }

    public abstract void Process(in WeaponContext ctx, uint time);

    /// <summary>
    ///     A running press or reload doesn't carry over.
    /// </summary>
    public abstract void Switch(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details, bool weaponChanged, bool linkedModes, uint time);

    protected static bool IsM1Chargeup(WeaponTemplateResult weapon) => (weapon.WeaponFlags & 7u) == 1u && weapon.MsChargeUp > 0;

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
}
