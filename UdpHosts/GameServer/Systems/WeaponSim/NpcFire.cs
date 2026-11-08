using System;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     An NPC has no client to report its shots, so the server times them and reloads on its own.
/// </summary>
internal sealed class NpcFire(WeaponSim sim) : WeaponFire(sim)
{
    // The client keeps all of it between linked modes of the same fire type
    private const float _chargeSwitchKeepFraction = 1.0f;

    private FireState _state = FireState.Idle;
    private uint _nextShotTime;

    // -1 for full auto, otherwise the shots left in the press
    private int _shotsRemainingInBurst;

    // For the spread heat, 0 for full auto
    private uint _totalRoundsInPress;

    private uint _reloadCompleteTime;
    private bool _triggerHeld;

    private uint _triggerHeldTime;

    // In ms
    private uint _charge;
    private uint _lastChargeTime;
    private uint _chargeCap;
    private uint _chargeMin;

    private enum FireState
    {
        Idle,
        Firing,
        Reloading,
    }

    public override void Switch(in WeaponContext ctx, bool weaponChanged, bool linkedModes, uint time)
    {
        _state = FireState.Idle;
        _nextShotTime = 0;
        _shotsRemainingInBurst = 0;
        _totalRoundsInPress = 0;
        _reloadCompleteTime = 0;
        _triggerHeld = false;
        _triggerHeldTime = 0;

        if (weaponChanged)
        {
            _charge = 0;
            _lastChargeTime = time;
            (_chargeCap, _chargeMin) = ChargeupCaps(ctx);
        }
        else if (_charge > 0)
        {
            _charge = LinkedSameMode(ctx.Entity, ctx.Slot) ? (uint)(_charge * _chargeSwitchKeepFraction) : 0;
        }
    }

    public override void AdvanceTo(in WeaponContext ctx, uint time)
    {
        var weapon = ctx.Details.Weapon;
        UpdateCharge(weapon, time);

        // Before the loop, so a press held through the reload fires this tick
        if (_state == FireState.Reloading && time >= _reloadCompleteTime)
        {
            ctx.Ammo.Reload(ctx.Entity, _reloadCompleteTime);
            _state = FireState.Idle;

            if (_triggerHeld && IsContinuousFireMode(weapon))
            {
                uint readyTime = Math.Max(_nextShotTime, _triggerHeldTime);
                readyTime = Math.Max(readyTime, _reloadCompleteTime);
                if (IsM1Chargeup(weapon) && _charge < _chargeMin)
                {
                    _nextShotTime = readyTime + (_chargeMin - _charge);
                    Sim.Logger.Debug("[FireSim] Reload ended uncharged (server={ServerTime} press={PressTime} charge={Charge}/{Cap} min={Min} ready={ReadyTime})", Sim.CurrentTime, _triggerHeldTime, _charge, _chargeCap, _chargeMin, _nextShotTime);
                }
                else
                {
                    Sim.Logger.Debug("[FireSim] Resuming fire after reload (server={ServerTime} press={PressTime} ready={ReadyTime})", Sim.CurrentTime, _triggerHeldTime, readyTime);
                    StartFiring(ctx, readyTime);
                }
            }
        }

        if (_state == FireState.Idle
            && _triggerHeld
            && IsContinuousFireMode(weapon)
            && _triggerHeldTime < _nextShotTime
            && ctx.Ammo.Has(1)
            && _nextShotTime <= time)
        {
            Sim.Logger.Debug("[FireSim] Resuming fire at ready time (server={ServerTime} ready={ReadyTime} press={PressTime})", Sim.CurrentTime, _nextShotTime, _triggerHeldTime);
            StartFiring(ctx, Math.Max(_nextShotTime, _triggerHeldTime));
        }

        // A shot due at the same time as the next event comes first
        while (_state == FireState.Firing && _nextShotTime <= time)
        {
            if (!TryFireScheduledShot(ctx))
            {
                OnClipEmpty(ctx, _nextShotTime);
                break;
            }
        }
    }

    public override void Handle(in WeaponContext ctx, WeaponFireEvent evt)
    {
        var weapon = ctx.Details.Weapon;
        switch (evt.Type)
        {
            case WeaponFireEventType.FireBurst:
                // Also during a reload, firing resumes from this press afterwards
                _triggerHeld = true;
                _triggerHeldTime = evt.Time;

                if (_state == FireState.Reloading)
                {
                    break;
                }

                if (IsChargeOnlyMode(weapon, ctx.Details.WeaponProfile))
                {
                    break;
                }

                if (!ctx.Ammo.Has(1))
                {
                    if (!ctx.Ammo.EmptyHandled)
                    {
                        OnClipEmpty(ctx, evt.Time);
                    }

                    break;
                }

                if (IsM1Chargeup(weapon) && _charge < _chargeMin)
                {
                    _nextShotTime = evt.Time + (_chargeMin - _charge);
                    break;
                }

                if (_nextShotTime > evt.Time)
                {
                    break;
                }

                StartFiring(ctx, evt.Time);
                break;

            case WeaponFireEventType.FireEnd:
            case WeaponFireEventType.FireCancel:
                _triggerHeld = false;
                _shotsRemainingInBurst = 0;
                if (_state == FireState.Firing)
                {
                    _state = FireState.Idle;
                }

                break;
        }
    }

    private static bool IsContinuousFireMode(WeaponTemplateResult weapon)
    {
        // BoltAction isn't part of the fire type bits
        if (weapon.Flags.BoltAction)
        {
            return false;
        }

        var burstMode = weapon.Flags.Mode;
        return burstMode != WeaponFlags.BurstMode.M2 && burstMode != WeaponFlags.BurstMode.Hold;
    }

    /// <summary>
    ///     The spin-up alt fire, without clip or attack ability it only charges the weapon.
    /// </summary>
    private static bool IsChargeOnlyMode(WeaponTemplateResult weapon, WeaponProfile profile)
    {
        bool canFire = (profile.BaseClipSize > 0 && profile.MaxAmmo > 0) || weapon.AttackAbility != 0;
        return !canFire;
    }

    private void UpdateCharge(WeaponTemplateResult weapon, uint time)
    {
        // A late event can be older than the last advance
        if (time <= _lastChargeTime)
        {
            return;
        }

        uint dt = time - _lastChargeTime;
        _lastChargeTime = time;
        if (_chargeCap == 0 || _state == FireState.Reloading)
        {
            return;
        }

        uint was = _charge;
        if (_state == FireState.Firing)
        {
            _charge = _chargeCap;
        }
        else if (_triggerHeld)
        {
            _charge = was + dt > _chargeCap ? _chargeCap : was + dt;
        }
        else
        {
            _charge = was >= dt ? was - dt : 0;
        }

        if (_chargeMin > 0 && _triggerHeld && was < _chargeMin && _charge >= _chargeMin)
        {
            Sim.Logger.Debug("[FireSim] Charge armed weapon={Weapon} charge={Charge}/{Cap} min={Min} server={ServerTime}", weapon.DebugName, _charge, _chargeCap, _chargeMin, Sim.CurrentTime);
        }
    }

    // False when the clip couldn't pay for the shot or is empty afterwards
    private bool TryFireScheduledShot(in WeaponContext ctx)
    {
        // An NPC shot is a whole burst, capped at the rounds left like a player's
        var ammo = ctx.Ammo;
        if (!ammo.Has(1) || !ammo.TrySpend(ctx.Entity, ammo.HasAmmo ? Math.Min(BurstCost(ctx), ammo.Clip) : 0, _nextShotTime))
        {
            return false;
        }

        Sim.FireShot(ctx, _nextShotTime, ctx.Entity.AimDirection, null, "pose", _totalRoundsInPress);
        bool overheated = AddHeat(ctx, _nextShotTime);

        // Also after the last shot, a press in the down time would otherwise fire right away
        _nextShotTime += Math.Max(1u, ctx.Details.WeaponProfile.MsPerBurst);
        if (_shotsRemainingInBurst > 1)
        {
            _shotsRemainingInBurst--;
        }
        else if (_shotsRemainingInBurst >= 0)
        {
            _state = FireState.Idle;
        }

        if (overheated)
        {
            _state = FireState.Idle;
            _nextShotTime = Math.Max(_nextShotTime, OverheatEnd(ctx));
        }

        return ctx.Ammo.Has(1);
    }

    private void StartFiring(in WeaponContext ctx, uint time)
    {
        _state = FireState.Firing;
        _nextShotTime = time;

        int rounds = StartPress(ctx, time, out var fullAuto);
        _shotsRemainingInBurst = fullAuto ? -1 : rounds;
        _totalRoundsInPress = fullAuto ? 0 : (uint)rounds;
    }

    private void OnClipEmpty(in WeaponContext ctx, uint time)
    {
        _state = FireState.Idle;
        _shotsRemainingInBurst = 0;
        _triggerHeld = false;
        ClipEmpty(ctx, time);

        if (StartReload(ctx, time, out var completeTime))
        {
            _state = FireState.Reloading;
            _reloadCompleteTime = completeTime;
            _charge = 0;
        }
    }
}
