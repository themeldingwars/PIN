using System;
using GameServer.Entities.Character;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     The client times a player's shots, a server clip below the client's would be taken over right away.
/// </summary>
internal sealed class PlayerFire(WeaponSim sim) : WeaponFire(sim)
{
    // Slack before a shot is logged, or dropped during a reload
    private const int _toleranceMs = 10;

    private bool _triggerHeld;
    private bool _reloading;
    private uint _reloadCompleteTime;
    private uint _lastShotTime;
    private uint _shotsInPress;

    // For the spread heat, 0 for full auto
    private uint _roundsInPress;

    // 0 when there's no first shot left to check
    private uint _expectedFirstShot;

    public override bool Busy => _triggerHeld || _reloading;

    public override void Process(in WeaponContext ctx, uint time)
    {
        while (ctx.Events.Count > 0 && ctx.Events[0].Time <= time)
        {
            var evt = ctx.Events[0];
            ctx.Events.RemoveAt(0);
            FinishReload(ctx, evt.Time);
            Handle(ctx, evt);
        }

        FinishReload(ctx, time);
    }

    public override void Switch(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details, bool weaponChanged, bool linkedModes, uint time) => Reset();

    private void Reset()
    {
        _triggerHeld = false;
        _reloading = false;
        _lastShotTime = 0;
        _shotsInPress = 0;
        _roundsInPress = 0;
        _expectedFirstShot = 0;
    }

    private void Handle(in WeaponContext ctx, WeaponFireEvent evt)
    {
        switch (evt.Type)
        {
            case WeaponFireEventType.Shot:
                Shoot(ctx, evt);
                break;

            case WeaponFireEventType.FireBurst:
                _triggerHeld = true;
                _shotsInPress = 0;
                _expectedFirstShot = ExpectedFirstShot(ctx, evt.Time);
                if (!_reloading && !ctx.Ammo.Has(1) && !ctx.Ammo.EmptyHandled)
                {
                    ClipEmpty(ctx, evt.Time);
                }

                break;

            case WeaponFireEventType.FireEnd:
            case WeaponFireEventType.FireCancel:
                _triggerHeld = false;
                _expectedFirstShot = 0;
                break;

            case WeaponFireEventType.FireInputIgnored:
                if (evt.Value != 0)
                {
                    _triggerHeld = false;
                    _expectedFirstShot = 0;
                }

                break;

            case WeaponFireEventType.SelectWeapon:
            case WeaponFireEventType.SelectFireMode:
                Reset();
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

        int spacing = (int)Math.Max(1u, ctx.Details.WeaponProfile.MsPerBurst);
        if (_lastShotTime != 0)
        {
            int since = unchecked((int)(time - _lastShotTime));
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

        int perShot = Math.Max(1, (int)weapon.AmmoPerBurst);
        if (!ammo.Has(perShot))
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

        ammo.TrySpend(ctx.Entity, perShot);
        _lastShotTime = time;
        _shotsInPress++;
        Sim.FireShot(ctx, time, shot.Aim, shot.Velocity, "client", _roundsInPress);

        if (!ammo.Has(perShot))
        {
            ClipEmpty(ctx, time);
            _triggerHeld = false;
        }
    }

    private uint ExpectedFirstShot(in WeaponContext ctx, uint press)
    {
        uint expected = IsM1Chargeup(ctx.Details.Weapon) ? press + ChargeupCaps(ctx.Entity, ctx.Details).Min : press;
        if (_lastShotTime != 0)
        {
            expected = Math.Max(expected, _lastShotTime + Math.Max(1u, ctx.Details.WeaponProfile.MsPerBurst));
        }

        if (_reloading)
        {
            expected = Math.Max(expected, _reloadCompleteTime);
        }

        return expected;
    }

    private void FinishReload(in WeaponContext ctx, uint time)
    {
        if (_reloading && time >= _reloadCompleteTime)
        {
            _reloading = false;
            ctx.Ammo.Reload(ctx.Entity);
        }
    }
}
