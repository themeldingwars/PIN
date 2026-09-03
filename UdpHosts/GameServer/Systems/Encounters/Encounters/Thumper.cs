using System;
using System.Collections.Generic;
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Thumper;
using GameServer.Enums;

namespace GameServer.Systems.Encounters.Encounters;

public class Thumper : BaseEncounter, IInteractionHandler
{
    // Wave tuning. These constants control the attack pressure. A thumper
    // without defenders must survive a full cycle with damage, but it must
    // not die.
    private const uint FirstWaveDelayMs = 10_000;
    private const uint WaveIntervalMs = 30_000;
    private const int MaxAliveAttackers = 8;
    private const float SpawnRadiusMin = 25f;
    private const float SpawnRadiusMax = 35f;

    // Hive eruption. Aranhas stay below the ground until seismic activity
    // wakes them. A thumper inside an authored hive site (admin command
    // "hive add") replaces the small default waves with a dense and fast
    // eruption. The distance to the hive center controls the strength.
    private const uint HiveWaveIntervalMs = 8_000;
    private const uint HiveFirstWaveDelayMs = 3_000;
    private const int HiveMaxWaveSize = 16;

    // Armed wildlife monster ids from dbcharacter::Monster. These types are
    // faction 7 (gaea), which is hostile to accord in FactionRelations.
    private const uint CrystiteAranhaWorker = 1184;
    private const uint CrystiteAranhaSieger = 1198;
    private const uint SpittingThresher = 506;

    private static readonly uint _updateFrequency = ThumperState.THUMPING.CountdownTime() / 100;
    private readonly List<ulong> _attackerIds = new();
    private readonly ThumperEntity _thumper;
    private ulong _lastUpdate;
    private ulong _nextWaveAt;
    private int _waveNumber;

    public Thumper(IShard shard, ulong entityId, HashSet<INetworkPlayer> participants, ThumperEntity thumperEntity)
        : base(shard, entityId, participants)
    {
        _thumper = thumperEntity;

        Shard.EncounterMan.StartUpdatingEncounter(this);
    }

    public void OnInteraction(BaseEntity actingEntity, BaseEntity target)
    {
        switch ((ThumperState)_thumper.StateInfo.State)
        {
            case ThumperState.THUMPING:
                Shard.Abilities.HandleActivateAbility(Shard, _thumper, _thumper.CompletedAbility);

                _thumper.TransitionToState(ThumperState.LEAVING);
                break;
            case ThumperState.COMPLETED:
                _thumper.StateInfo = _thumper.StateInfo with { CountdownTime = Shard.CurrentTime };
                break;
        }
    }

    public override void OnUpdate(ulong currentTime)
    {
        if (_thumper.IsDestroyed)
        {
            OnFailure();
            return;
        }

        if (_thumper.StateInfo.State == (byte)ThumperState.THUMPING)
        {
            TrySpawnWave(currentTime);
        }

        if (Shard.CurrentTime >= _thumper.StateInfo.CountdownTime)
        {
            switch ((ThumperState)_thumper.StateInfo.State)
            {
                case ThumperState.LANDING:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, _thumper.LandedAbility);
                    break;
                case ThumperState.WARMINGUP:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34579);
                    break;
                case ThumperState.THUMPING:
                    _thumper.SetProgress(1);
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34215);
                    break;
                case ThumperState.CLOSING:
                    break;
                case ThumperState.COMPLETED:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34216);
                    break;
                case ThumperState.LEAVING:
                    OnSuccess();
                    break;
            }

            if (_thumper.StateInfo.State < (byte)ThumperState.LEAVING)
            {
                _thumper.TransitionToState((ThumperState)(_thumper.StateInfo.State + 1));
            }
        }
        else if (_thumper.StateInfo.State == (byte)ThumperState.THUMPING && currentTime > _lastUpdate + _updateFrequency)
        {
            _thumper.SetProgress((float)(Shard.CurrentTime - _thumper.StateInfo.Time)
                                / (_thumper.StateInfo.CountdownTime - _thumper.StateInfo.Time));

            _lastUpdate = currentTime;
        }
    }

    public override void OnSuccess()
    {
        CleanupAttackers();

        Shard.EncounterMan.StopUpdatingEncounter(this);

        Shard.EntityMan.Remove(_thumper);

        base.OnSuccess();
    }

    /// <summary>The thumper is destroyed. Remove the attackers and the thumper.</summary>
    public override void OnFailure()
    {
        CleanupAttackers();

        Shard.EncounterMan.StopUpdatingEncounter(this);

        Shard.EntityMan.Remove(_thumper);

        base.OnFailure();
    }

    /// <summary>
    ///     Spawns attack waves while the thumper drills. The waves grow with
    ///     the wave number. Each wave spawns on a ring around the thumper.
    ///     One half of each wave gets an order to attack the thumper. The
    ///     other half uses the default AI behavior, which prefers players.
    /// </summary>
    private void TrySpawnWave(ulong currentTime)
    {
        var influence = Shard.Hives.InfluenceAt(_thumper.Position, out var hive);

        if (_nextWaveAt == 0)
        {
            // Near a hive center, the first eruption comes fast
            _nextWaveAt = currentTime + (influence > 0.4f ? HiveFirstWaveDelayMs : FirstWaveDelayMs);
            return;
        }

        if (currentTime < _nextWaveAt)
        {
            return;
        }

        // A position nearer to the hive center gives faster waves
        var interval = WaveIntervalMs - (ulong)((WaveIntervalMs - HiveWaveIntervalMs) * influence);
        _nextWaveAt = currentTime + interval;
        _waveNumber++;

        // Remove the ids of attackers that no longer exist
        _attackerIds.RemoveAll(id => !Shard.Entities.ContainsKey(id));

        var maxAlive = MaxAliveAttackers;
        if (hive != null && influence > 0f)
        {
            maxAlive = Math.Max(MaxAliveAttackers, (int)(hive.Burst * influence));
        }

        var budget = maxAlive - _attackerIds.Count;
        var waveSize = Math.Min(Math.Min(2 + _waveNumber + (int)(influence * 12), HiveMaxWaveSize), budget);

        for (var i = 0; i < waveSize; i++)
        {
            // The first slots of later waves carry the heavy types
            var typeId = CrystiteAranhaWorker;
            if (_waveNumber >= 2 && i == 0)
            {
                typeId = CrystiteAranhaSieger;
            }
            else if (_waveNumber >= 4 && i == 1)
            {
                typeId = SpittingThresher;
            }

            var angle = Rng.NextDouble() * Math.PI * 2;
            var radius = SpawnRadiusMin + ((float)Rng.NextDouble() * (SpawnRadiusMax - SpawnRadiusMin));
            var offset = new Vector3((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius, 0.5f);

            // Put the spawn ring on the terrain. The thumper Z plus a flat
            // offset is wrong when the thumper is on a slope.
            var spawnPosition = _thumper.Position + offset;
            var ground = Shard.Physics.GetGroundHeight(spawnPosition);
            if (ground.HasValue)
            {
                spawnPosition.Z = ground.Value + 0.5f;
            }

            var attacker = Shard.EntityMan.SpawnCharacter(typeId, spawnPosition);
            _attackerIds.Add(attacker.EntityId);

            if (i % 2 == 0)
            {
                Shard.AI.IssueOrder(attacker.EntityId, new AttackEntityOrder(_thumper.EntityId));
            }
        }
    }

    private void CleanupAttackers()
    {
        foreach (var id in _attackerIds)
        {
            Shard.EntityMan.Remove(id);
        }
    }
}