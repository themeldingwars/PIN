using System;
using GameServer.Entities.Character;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     The client times a player's shots, a server clip below the client's would be taken over right away.
/// </summary>
internal sealed class PlayerFire(WeaponSim sim) : WeaponFire(sim)
{
    // Clock jitter a shot may have before it counts as early
    private const int _toleranceMs = 10;

    private bool _reloading;
    private uint _reloadCompleteTime;
    private uint _shotsInPress;

    // For the spread heat, 0 for full auto
    private uint _roundsInPress;

    // 0 when there's no first shot left to check
    private uint _expectedFirstShot;

    public override void AdvanceTo(in WeaponContext ctx, uint time) => FinishReload(ctx, time);

    // Holstering cancels the reload like in the client, a switch between linked modes keeps it
    public override void Switch(in WeaponContext ctx, bool weaponChanged, bool linkedModes, uint time)
    {
        _reloading &= !weaponChanged && linkedModes;
        _shotsInPress = 0;
        _roundsInPress = 0;
        _expectedFirstShot = 0;
    }

    public override void Handle(in WeaponContext ctx, WeaponFireEvent evt)
    {
        switch (evt.Type)
        {
            case WeaponFireEventType.Shot:
                Shoot(ctx, evt);
                break;

            case WeaponFireEventType.FireBurst:
                _shotsInPress = 0;
                _expectedFirstShot = ExpectedFirstShot(ctx, evt.Time);
                if (!_reloading && !ctx.Ammo.Has(1) && !ctx.Ammo.EmptyHandled)
                {
                    ClipEmpty(ctx, evt.Time);
                }

                break;

            case WeaponFireEventType.FireEnd:
            case WeaponFireEventType.FireCancel:
                _expectedFirstShot = 0;
                break;

            case WeaponFireEventType.FireInputIgnored:
                if (evt.Value != 0)
                {
                    _expectedFirstShot = 0;
                }

                break;

            case WeaponFireEventType.ReloadWeapon:
                if (_reloading)
                {
                    break;
                }

                if (StartReload(ctx, evt.Time, out var completeTime))
                {
                    _reloading = true;
                    _reloadCompleteTime = completeTime;
                }
                else
                {
                    Sim.Logger.Debug("[FireCheck] Reload with clip={Clip}/{ClipMax} reserve={Reserve} (weapon={Weapon})", ctx.Ammo.Clip, ctx.Ammo.ClipMax, ctx.Ammo.Reserve, ctx.Details.Weapon.DebugName);
                }

                break;

            case WeaponFireEventType.CancelReload:
                _reloading = false;
                break;
        }
    }

    private void Shoot(in WeaponContext ctx, WeaponFireEvent shot)
    {
        var weapon = ctx.Details.Weapon;
        var ammo = ctx.Ammo;
        uint time = shot.Time;

        if (_reloading)
        {
            int early = unchecked((int)(_reloadCompleteTime - time));
            if (early > _toleranceMs)
            {
                Sim.Logger.Debug("[FireCheck] Shot {Early} ms before the reload ends (weapon={Weapon})", early, weapon.DebugName);
                return;
            }

            FinishReload(ctx, _reloadCompleteTime);
        }

        // Only logged, the client scales the swap by a factor we don't know yet: a 700 ms swap fired after 658 ms
        if (time + _toleranceMs < ctx.SwapReady)
        {
            Sim.Logger.Debug("[FireCheck] Shot {Early} ms before the weapon swap ends (weapon={Weapon})", ctx.SwapReady - time, weapon.DebugName);
        }

        if (time + _toleranceMs < OverheatEnd(ctx))
        {
            Sim.Logger.Debug("[FireCheck] Shot {Early} ms before the overheat ends (weapon={Weapon})", OverheatEnd(ctx) - time, weapon.DebugName);
            return;
        }

        int spacing = (int)ShotSpacing(ctx, _shotsInPress);
        uint lastShot = ctx.Mode.LastShotTime;
        if (lastShot != 0)
        {
            int since = unchecked((int)(time - lastShot));
            if (since < spacing - _toleranceMs)
            {
                Sim.Logger.Debug("[FireCheck] Shot {Since} ms after the last one, the weapon needs {Spacing} ms (weapon={Weapon})", since, spacing, weapon.DebugName);

                // Too fast for timing jitter
                if (since < spacing / 2)
                {
                    return;
                }
            }
        }

        // The client caps the cost at the rounds left, one is enough to fire, a free shot needs none
        int cost = ShotCost(ctx, _shotsInPress);
        if (cost > 0 && !ammo.Has(1))
        {
            Sim.Logger.Debug("[FireCheck] Shot with {Clip} rounds in the clip (weapon={Weapon})", ammo.Clip, weapon.DebugName);
            return;
        }

        if (_expectedFirstShot != 0)
        {
            int off = unchecked((int)(time - _expectedFirstShot));
            if (Math.Abs(off) > _toleranceMs)
            {
                Sim.Logger.Debug("[FireCheck] First shot of the press {Off} ms off the expected time (weapon={Weapon})", off, weapon.DebugName);
            }

            _expectedFirstShot = 0;
        }

        if (_shotsInPress == 0)
        {
            int rounds = StartPress(ctx, time, out var fullAuto);
            _roundsInPress = fullAuto ? 0 : (uint)rounds;
        }

        ammo.TrySpend(ctx.Entity, Math.Min(cost, ammo.HasAmmo ? ammo.Clip : int.MaxValue), time);
        RecordShot(ctx, time);
        _shotsInPress++;
        Sim.FireShot(ctx, time, shot.Aim, shot.Velocity, "client", _roundsInPress);
        AddHeat(ctx, time);

        if (cost > 0 && !ammo.Has(1))
        {
            ClipEmpty(ctx, time);
        }
    }

    private uint ExpectedFirstShot(in WeaponContext ctx, uint press)
    {
        uint expected = IsM1Chargeup(ctx.Details.Weapon) ? press + ChargeupCaps(ctx).Min : press;
        if (ctx.Mode.LastShotTime != 0)
        {
            expected = Math.Max(expected, ctx.Mode.LastShotTime + ShotSpacing(ctx, 0));
        }

        if (_reloading)
        {
            expected = Math.Max(expected, _reloadCompleteTime);
        }

        return Math.Max(expected, OverheatEnd(ctx));
    }

    private void FinishReload(in WeaponContext ctx, uint time)
    {
        if (_reloading && time >= _reloadCompleteTime)
        {
            _reloading = false;
            ctx.Ammo.Reload(ctx.Entity, _reloadCompleteTime);
        }
    }
}
