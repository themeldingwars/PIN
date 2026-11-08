using System;
using GameServer.Entities.Character;
using Serilog;

namespace GameServer.Systems.WeaponSim;

/// <summary>
///     The only code that writes the clip and ammo fields of the combat controller and view.
/// </summary>
public sealed class WeaponAmmo
{
    private const uint _regenDelayMs = 500;

    private readonly State[,] _states = new State[3, 2];

    // Acts as a weapon without ammo until the first Select
    private State _active = new();

    public bool HasAmmo => _active.HasAmmo;

    public int Clip => _active.Clip;

    public int ClipMax => _active.ClipMax;

    public int Reserve => _active.Reserve;

    /// <summary>
    ///     The empty clip's ability already ran, false again once the clip has rounds.
    /// </summary>
    public bool EmptyHandled => _active.EmptyHandled;

    // 0 before the first shot
    public uint LastShotTime => _active.LastShotTime;

    public bool CanReload => _active.HasAmmo && _active.Reserve > 0 && _active.Clip < _active.ClipMax;

    /// <summary>
    ///     Always true for a weapon without ammo.
    /// </summary>
    public bool Has(int rounds) => !_active.HasAmmo || _active.Clip >= rounds;

    /// <summary>
    ///     Linked modes share the main mode's clip, a different weapon in the slot starts full.
    /// </summary>
    public void Select(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails active, byte mode, uint time, ILogger logger)
    {
        var other = entity.GetWeaponDetails((byte)(1 - mode));
        var linked = active.Weapon.Flags.LinkedModes || (other?.Weapon != null && other.Weapon.Flags.LinkedModes);
        var stateMode = linked ? (byte)0 : mode;
        var details = linked && mode != 0 && entity.GetWeaponDetails(0) is { Weapon: not null } main ? main : active;
        var slot = entity.WeaponIndex.Index;
        if (_states[slot, stateMode] is { } state && state.WeaponId == details.WeaponId)
        {
            _active = state;
            return;
        }

        var clip = WeaponAttributes.ClipSize(entity, details);
        var max = WeaponAttributes.MaxAmmo(entity, details);
        state = new State
        {
            WeaponId = details.WeaponId,
            Slot = slot,
            Mode = stateMode,
            Shared = linked,
            HasAmmo = clip > 0 && max > 0,
            ClipMax = clip,
            ReserveMax = FullReserve(clip, max),
            Clip = clip,
            Reserve = FullReserve(clip, max),
            RegenMs = WeaponAttributes.RegenInterval(entity, details),
            LastRegenTime = time,
        };
        _states[slot, stateMode] = state;
        _active = state;
        logger.Debug("[FireSim] Ammo initialized weapon={Weapon} slot={Slot} mode={Mode} clip={Clip} reserve={Reserve}", details.Weapon.DebugName, slot, stateMode, state.Clip, state.Reserve);
    }

    /// <summary>
    ///     Takes nothing when the clip is short. Always true for a weapon without ammo.
    /// </summary>
    public bool TrySpend(CharacterEntity entity, int rounds, uint time)
    {
        if (!_active.HasAmmo)
        {
            return true;
        }

        if (_active.Clip < rounds)
        {
            return false;
        }

        _active.Clip -= rounds;
        _active.LastShotTime = time;
        Write(entity, _active);
        return true;
    }

    public void MarkEmpty() => _active.EmptyHandled = true;

    public void Reload(CharacterEntity entity, uint time)
    {
        int rounds = Math.Min(_active.ClipMax - _active.Clip, _active.Reserve);
        _active.Clip += rounds;
        _active.Reserve -= rounds;
        _active.ReloadTime = time;
        if (_active.Clip > 0)
        {
            _active.EmptyHandled = false;
        }

        Write(entity, _active);
    }

    /// <summary>
    ///     Every weapon used so far regenerates, also the holstered one, like in the client.
    /// </summary>
    public void Regen(CharacterEntity entity, uint time)
    {
        foreach (var state in _states)
        {
            if (state != null)
            {
                Regen(entity, state, time);
            }
        }
    }

    /// <summary>
    ///     Forgets every weapon and writes full clips and reserves.
    /// </summary>
    public void Reset(CharacterEntity entity)
    {
        Array.Clear(_states);
        _active = new State();

        // Weapon slot 1 (primary) maps to ammo slot 0, weapon slot 2 (secondary) to ammo slot 1
        ResetSlot(entity, 1, 0);
        ResetSlot(entity, 2, 1);
    }

    /// <summary>
    ///     Refills only the reserve, up to the capacity minus the clip, like the live servers. Takes the share of that room in
    ///     percent, 100 for a full restock.
    /// </summary>
    public void Restock(CharacterEntity entity, uint percent)
    {
        // A weapon that was never selected still has the full ammo from Reset
        foreach (var state in _states)
        {
            if (state is not { HasAmmo: true })
            {
                continue;
            }

            int room = Math.Max(0, state.ClipMax + state.ReserveMax - state.Clip);
            int reserve = percent >= 100 ? room : Math.Min(room, state.Reserve + (int)((((long)room * percent) + 99) / 100));
            if (reserve > state.Reserve)
            {
                state.Reserve = reserve;
                Write(entity, state, reserveOnly: true);
            }
        }
    }

    /// <summary>
    ///     Takes the weapon slot, 1 or 2. A weapon that was never selected counts as full.
    /// </summary>
    public bool NeedsAmmo(byte weaponSlot)
    {
        if (weaponSlot >= _states.GetLength(0))
        {
            return false;
        }

        for (var mode = 0; mode < _states.GetLength(1); mode++)
        {
            if (_states[weaponSlot, mode] is { HasAmmo: true } state && NeedsAmmo(state.Clip + state.Reserve, state.ClipMax + state.ReserveMax))
            {
                return true;
            }
        }

        return false;
    }

    private static bool NeedsAmmo(int ammo, int max) => ammo < max && (max < 10000 || ammo <= max / 2);

    // MaxAmmo includes the clip
    private static ushort FullReserve(ushort clip, ushort max) => (ushort)Math.Max(0, max - clip);

    private static void ResetSlot(CharacterEntity entity, byte weaponSlot, int ammoSlot)
    {
        var details = entity.GetWeaponDetails(weaponSlot, 0);
        if (details?.Weapon == null)
        {
            return;
        }

        var clip = WeaponAttributes.ClipSize(entity, details);
        var reserve = FullReserve(clip, WeaponAttributes.MaxAmmo(entity, details));
        WriteFields(entity, ammoSlot, false, clip, reserve);
        if (entity.GetWeaponDetails(weaponSlot, 1) is not { Weapon: not null } alt)
        {
            return;
        }

        // Linked modes share the main's clip
        if (details.Weapon.Flags.LinkedModes || alt.Weapon.Flags.LinkedModes)
        {
            WriteFields(entity, ammoSlot, true, clip, reserve);
            return;
        }

        var altClip = WeaponAttributes.ClipSize(entity, alt);
        WriteFields(entity, ammoSlot, true, altClip, FullReserve(altClip, WeaponAttributes.MaxAmmo(entity, alt)));
    }

    private static void WriteFields(CharacterEntity entity, int ammoSlot, bool alt, ushort? clip, ushort reserve)
    {
        var view = entity.Character_CombatView;
        var controller = entity.Character_CombatController;
        switch ((ammoSlot, alt))
        {
            case (0, false):
                if (view != null)
                {
                    view.Ammo_0Prop = reserve;
                }

                if (controller != null)
                {
                    if (clip.HasValue)
                    {
                        controller.Clip_0Prop = clip.Value;
                    }

                    controller.Ammo_0Prop = reserve;
                }

                break;
            case (1, false):
                if (view != null)
                {
                    view.Ammo_1Prop = reserve;
                }

                if (controller != null)
                {
                    if (clip.HasValue)
                    {
                        controller.Clip_1Prop = clip.Value;
                    }

                    controller.Ammo_1Prop = reserve;
                }

                break;
            case (0, true):
                if (view != null)
                {
                    view.AltAmmo_0Prop = reserve;
                }

                if (controller != null)
                {
                    if (clip.HasValue)
                    {
                        controller.AltClip_0Prop = clip.Value;
                    }

                    controller.AltAmmo_0Prop = reserve;
                }

                break;
            case (1, true):
                if (view != null)
                {
                    view.AltAmmo_1Prop = reserve;
                }

                if (controller != null)
                {
                    if (clip.HasValue)
                    {
                        controller.AltClip_1Prop = clip.Value;
                    }

                    controller.AltAmmo_1Prop = reserve;
                }

                break;
        }
    }

    // A restock sends only the reserve, a clip would reset the client's prediction mid burst
    private static void Write(CharacterEntity entity, State state, bool reserveOnly = false)
    {
        if (!state.HasAmmo)
        {
            return;
        }

        var ammoSlot = state.Slot >= 2 ? 1 : 0;
        ushort? clip = reserveOnly ? null : (ushort)Math.Max(0, state.Clip);
        var reserve = (ushort)Math.Max(0, state.Reserve);
        WriteFields(entity, ammoSlot, state.Mode == 1, clip, reserve);
        if (state.Shared)
        {
            WriteFields(entity, ammoSlot, true, clip, reserve);
        }
    }

    private static void Regen(CharacterEntity entity, State state, uint time)
    {
        if (!state.HasAmmo || state.RegenMs == 0 || state.Clip >= state.ClipMax)
        {
            return;
        }

        // All due rounds at once like the client, one per tick would fall behind its clip
        uint start = Math.Max(Math.Max(state.LastShotTime, state.ReloadTime) + _regenDelayMs, state.LastRegenTime);
        if (time < start + state.RegenMs)
        {
            return;
        }

        uint due = (time - start) / state.RegenMs;
        state.LastRegenTime = start + (due * state.RegenMs);
        int rounds = (int)Math.Min(due, (uint)Math.Min(state.ClipMax - state.Clip, state.Reserve));
        if (rounds <= 0)
        {
            return;
        }

        state.Clip += rounds;
        state.Reserve -= rounds;
        Write(entity, state);
    }

    private sealed class State
    {
        public uint WeaponId;
        public byte Slot;
        public byte Mode;
        public bool Shared;
        public bool HasAmmo;
        public bool EmptyHandled;
        public int Clip;
        public int Reserve;
        public int ClipMax;
        public int ReserveMax;
        public uint LastRegenTime;
        public uint LastShotTime;
        public uint ReloadTime;
        public uint RegenMs;
    }
}
