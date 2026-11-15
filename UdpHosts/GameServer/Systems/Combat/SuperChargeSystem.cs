using System;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.Enums;
using Serilog;

namespace GameServer.Systems.Combat;

public class SuperChargeSystem
{
    public const float MeterMax = 100f;
    public const float AbilityActivationGain = 0.25f;
    private const float _dealtHitGainCap = 1f;
    private const ulong _syncIntervalMs = 1_020;
    private const ulong _meterRetentionMs = 10 * 60_000;
    private const ulong _pruneIntervalMs = 60_000;

    private const byte _opEvent = 1;
    private const byte _opSyncFirst = 0;
    private const byte _opSyncSecond = 2;

    private readonly Shard _shard;
    private readonly ILogger _logger;
    private readonly Dictionary<ulong, Meter> _meters = [];
    private readonly HashSet<ulong> _trackedThisTick = [];
    private ulong _lastPruneTime;
    private SuperChargeTuning _tuning;

    public SuperChargeSystem(Shard shard)
    {
        _shard = shard;
        _logger = shard.Logger.ForContext<SuperChargeSystem>();
    }

    private SuperChargeTuning Tuning => _tuning ??= SuperChargeTuning.FromStaticDb();

    public float GetCharge(CharacterEntity character)
    {
        return _meters.TryGetValue(character.EntityId, out var meter)
            ? meter.Value
            : (float)character.Character_CombatController.SuperChargeProp.Value;
    }

    public void OnDamageDealt(CharacterEntity dealer, int damage)
    {
        if (!dealer.IsPlayerControlled || damage <= 0)
        {
            return;
        }

        var meter = OnCombatAction(dealer);
        AddCharge(character: dealer, meter, Math.Min(_dealtHitGainCap, damage * Tuning.PercentPerDamageDealt * MeterMax));
    }

    public void OnDamageTaken(CharacterEntity victim, int damage)
    {
        if (!victim.IsPlayerControlled || damage <= 0)
        {
            return;
        }

        var meter = OnCombatAction(victim);
        AddCharge(character: victim, meter, damage * Tuning.PercentPerDamageTaken * MeterMax);
    }

    public void OnHealDealt(CharacterEntity healer, int amount)
    {
        if (!healer.IsPlayerControlled || amount <= 0)
        {
            return;
        }

        AddCharge(healer, GetOrCreate(healer.EntityId), amount * Tuning.PercentPerHealDealt * MeterMax);
    }

    public void OnAbilityActivated(CharacterEntity character)
    {
        if (!character.IsPlayerControlled)
        {
            return;
        }

        AddCharge(character, GetOrCreate(character.EntityId), AbilityActivationGain);
    }

    public void AddAbilityGain(CharacterEntity character, float points)
    {
        if (!character.IsPlayerControlled)
        {
            return;
        }

        AddCharge(character, GetOrCreate(character.EntityId), points);
    }

    public void ConsumePercentOfCurrent(CharacterEntity character, float percent)
    {
        if (!character.IsPlayerControlled || percent == 0)
        {
            return;
        }

        var meter = GetOrCreate(character.EntityId);
        meter.Value = percent > 0
            ? meter.Value - (percent / 100f * meter.Value)
            : meter.Value - percent;
        meter.Value = Math.Clamp(meter.Value, 0f, MeterMax);
        WriteMeter(character, meter, _opEvent);
    }

    public void SetCharge(CharacterEntity character, float percent)
    {
        if (!character.IsPlayerControlled)
        {
            return;
        }

        var meter = GetOrCreate(character.EntityId);
        meter.Value = Math.Clamp(percent, 0f, MeterMax);

        meter.LastCombatTime = _shard.CurrentTimeLong;
        WriteMeter(character, meter, _opEvent);
        _logger.Information("{Character} supercharge set to {Charge}%", character, meter.Value);
    }

    public void ResetMeter(CharacterEntity character, string reason)
    {
        if (!character.IsPlayerControlled)
        {
            return;
        }

        var meter = GetOrCreate(character.EntityId);
        if (meter.Value == 0)
        {
            return;
        }

        meter.Value = 0;
        WriteMeter(character, meter, _opEvent);
        _logger.Information("{Character} supercharge reset on {Reason}", character, reason);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (_meters.Count == 0)
        {
            return;
        }

        var deltaSeconds = (float)(deltaTime / 1000.0);
        _trackedThisTick.Clear();

        foreach (var client in _shard.Clients.Values)
        {
            if (client.CharacterEntity is not { IsPlayerControlled: true })
            {
                continue;
            }

            var character = client.CharacterEntity;
            var meter = GetOrCreate(character.EntityId);
            _trackedThisTick.Add(character.EntityId);
            meter.LastSeenTime = currentTime;

            if (meter.Value <= 0 && meter.Value == meter.LastSyncedDisplay)
            {
                continue;
            }

            var sinceCombat = currentTime - meter.LastCombatTime;
            var tuning = Tuning;
            if (sinceCombat < tuning.CombatTimeoutMs)
            {
                if (meter.Value < MeterMax && CanGenerate(character) && tuning.IdleGainDurationMs > 0)
                {
                    var regenPerSecond = tuning.IdleGainPercent / (tuning.IdleGainDurationMs / 1000f) * GetChargeSpeedScale(character);
                    meter.Value = Math.Min(MeterMax, meter.Value + (regenPerSecond * deltaSeconds));
                }
            }
            else if (sinceCombat >= tuning.IdleHoldDurationMs && meter.Value > 0)
            {
                var decayedMs = sinceCombat - tuning.IdleHoldDurationMs;
                var ramp = tuning.IdleMaxDecayDurationMs > 0 ? Math.Min(1f, decayedMs / tuning.IdleMaxDecayDurationMs) : 1f;
                var rate = tuning.IdleMaxDecayRate * ramp;
                meter.Value = Math.Max(0f, meter.Value - (rate * deltaSeconds));
            }

            var displayed = Display(meter.Value);
            if (displayed != meter.LastSyncedDisplay && currentTime - meter.LastSyncTime >= _syncIntervalMs)
            {
                meter.SyncOp = meter.SyncOp == _opSyncFirst ? _opSyncSecond : _opSyncFirst;
                WriteMeter(character, meter, meter.SyncOp);
            }
        }

        if (currentTime - _lastPruneTime >= _pruneIntervalMs)
        {
            PruneMeters(currentTime);
            _lastPruneTime = currentTime;
        }
    }

    private static void WriteMeter(CharacterEntity character, Meter meter, byte op)
    {
        meter.Value = Math.Clamp(meter.Value, 0f, MeterMax);
        character.Character_CombatController.SuperChargeProp = new SuperChargeData { Value = meter.Value, Op = op };
        meter.LastSyncedDisplay = Display(meter.Value);
        meter.LastSyncTime = character.Shard.CurrentTimeLong;
    }

    private static float Display(float value) => (float)(HalfFloat)value;

    private static bool CanGenerate(CharacterEntity character)
    {
        return !character.CurrentPermissions.TryGetValue(PermissionFlagsData.CharacterPermissionFlags.unk_21, out var allowed) || allowed;
    }

    private static float GetChargeSpeedScale(CharacterEntity character)
    {
        var scale = character.GetItemAttribute((ushort)ItemAttributeId.ChargeSpeedHkm);
        return scale > 0 ? scale : 1f;
    }

    private Meter OnCombatAction(CharacterEntity character)
    {
        var meter = GetOrCreate(character.EntityId);
        meter.LastCombatTime = _shard.CurrentTimeLong;
        return meter;
    }

    private void AddCharge(CharacterEntity character, Meter meter, float points)
    {
        if (points <= 0 || meter.Value >= MeterMax || !CanGenerate(character))
        {
            return;
        }

        meter.Value = Math.Min(MeterMax, meter.Value + points);
        WriteMeter(character, meter, _opEvent);
    }

    private Meter GetOrCreate(ulong entityId)
    {
        if (!_meters.TryGetValue(entityId, out var meter))
        {
            meter = new Meter { LastSeenTime = _shard.CurrentTimeLong };
            _meters[entityId] = meter;
        }

        return meter;
    }

    private void PruneMeters(ulong currentTime)
    {
        List<ulong> stale = null;
        foreach (var (entityId, meter) in _meters)
        {
            if (!_trackedThisTick.Contains(entityId) && currentTime - meter.LastSeenTime >= _meterRetentionMs)
            {
                stale ??= [];
                stale.Add(entityId);
            }
        }

        if (stale != null)
        {
            foreach (var entityId in stale)
            {
                _meters.Remove(entityId);
            }
        }
    }

    private sealed class Meter
    {
        public float Value;
        public float LastSyncedDisplay;
        public ulong LastCombatTime;
        public ulong LastSyncTime;
        public ulong LastSeenTime;
        public byte SyncOp = _opSyncFirst;
    }
}
