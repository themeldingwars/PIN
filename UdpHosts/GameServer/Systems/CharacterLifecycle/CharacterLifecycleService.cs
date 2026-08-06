using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.GSS.V66.Character;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.CharacterLifecycle;

public class CharacterLifecycleService
{
    private static readonly ILogger Logger = Log.ForContext<CharacterLifecycleService>();

    private readonly IShard _shard;
    private readonly IEventBus _eventBus;
    private readonly ICharacterLifecycleRules _rules;
    private readonly IDictionary<ulong, LifecycleTracker> _trackers = new ConcurrentDictionary<ulong, LifecycleTracker>();

    public CharacterLifecycleService(IShard shard, IEventBus eventBus, ICharacterLifecycleRules rules)
    {
        _shard = shard;
        _eventBus = eventBus;
        _rules = rules;

        _eventBus.Subscribe<EntityDamagedEvent>(OnEntityDamaged);
    }

    public void OnCharacterCreated(CharacterEntity character)
    {
        if (!_trackers.ContainsKey(character.EntityId))
        {
            _trackers[character.EntityId] = new LifecycleTracker
            {
                State = CharacterLifecycleState.Living,
                BleedoutStartTime = 0,
                DeadStartTime = 0,
            };
        }
    }

    public void OnCharacterRemoved(CharacterEntity character)
    {
        _trackers.Remove(character.EntityId);
    }

    public CharacterLifecycleState GetState(CharacterEntity character)
    {
        if (_trackers.TryGetValue(character.EntityId, out var tracker))
        {
            return tracker.State;
        }

        return CharacterLifecycleState.Living;
    }

    public void Reset(CharacterEntity character)
    {
        if (_trackers.TryGetValue(character.EntityId, out var tracker))
        {
            tracker.State = CharacterLifecycleState.Living;
            tracker.BleedoutStartTime = 0;
            tracker.DeadStartTime = 0;
            tracker.LastBleedoutDamage = 0;
        }
    }

    public void ForceBleedout(CharacterEntity character)
    {
        character.SetCurrentHealth(0);
        TransitionToBleedout(character);
    }

    public void ForceDeath(CharacterEntity character)
    {
        character.SetCurrentHealth(0);
        TransitionToDead(character);
    }

    public void TryRevive(CharacterEntity target, IEntity reviver = null)
    {
        if (!_trackers.TryGetValue(target.EntityId, out var tracker))
        {
            Logger.Warning("Cannot revive {Name}: no lifecycle tracker", target);
            return;
        }

        if (tracker.State != CharacterLifecycleState.Bleedout)
        {
            Logger.Warning("Cannot revive {Name}: not in bleedout state, current state is {State}", target, tracker.State);
            return;
        }

        tracker.State = CharacterLifecycleState.Living;
        tracker.BleedoutStartTime = 0;
        tracker.DeadStartTime = 0;

        target.SetCharacterState(CharacterStateData.CharacterStatus.Living, target.Shard.CurrentTime);
        target.SetCurrentHealth(int.Min(target.CurrentHealth + (target.MaxHealth.Value / 4), target.MaxHealth.Value));

        if (reviver != null)
        {
            Logger.Information("{Name} revived by {Reviver}", target, reviver.ToString());
        }
        else
        {
            Logger.Information("{Name} revived.", target);
        }

        _eventBus.Publish(new CharacterRevivedEvent(target, reviver));
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        foreach (var kvp in _trackers)
        {
            var tracker = kvp.Value;
            if (tracker.State != CharacterLifecycleState.Bleedout)
            {
                continue;
            }

            if (!_shard.Entities.TryGetValue(kvp.Key, out var entity) || entity is not CharacterEntity character)
            {
                _trackers.Remove(kvp.Key);
                continue;
            }

            HandleBleedoutTick(tracker, character);
        }
    }

    private void OnEntityDamaged(EntityDamagedEvent evt)
    {
        if (evt.Target is not CharacterEntity character)
        {
            return;
        }

        if (character.CurrentHealth <= 0)
        {
            if (_rules.BleedoutEnabled && _rules.CanBleedout(character))
            {
                TransitionToBleedout(character);
            }
            else
            {
                TransitionToDead(character);
            }
        }
    }

    private void TransitionToBleedout(CharacterEntity character)
    {
        if (!_trackers.TryGetValue(character.EntityId, out var tracker))
        {
            return;
        }

        if (tracker.State == CharacterLifecycleState.Bleedout)
        {
            return;
        }

        tracker.State = CharacterLifecycleState.Bleedout;
        tracker.BleedoutStartTime = character.Shard.CurrentTimeLong;

        character.SetCharacterState(CharacterStateData.CharacterStatus.Incapacitated, character.Shard.CurrentTime);

        Logger.Information("{Name} entered bleedout", character);
        _eventBus.Publish(new CharacterEnteredBleedoutEvent(character, tracker.BleedoutStartTime + (ulong)_rules.BleedoutDurationMs));
    }

    private void TransitionToDead(CharacterEntity character)
    {
        if (!_trackers.TryGetValue(character.EntityId, out var tracker))
        {
            return;
        }

        if (tracker.State == CharacterLifecycleState.Dead)
        {
            return;
        }

        tracker.State = CharacterLifecycleState.Dead;
        tracker.DeadStartTime = character.Shard.CurrentTimeLong;

        character.SetCharacterState(CharacterStateData.CharacterStatus.Dead, character.Shard.CurrentTime);
        character.Alive = false;

        Logger.Information("{Name} died", character);
        _eventBus.Publish(new CharacterDiedEvent(character));
    }

    private void HandleBleedoutTick(LifecycleTracker tracker, CharacterEntity character)
    {
        if (tracker.BleedoutStartTime == 0)
        {
            return;
        }

        ulong elapsed = character.Shard.CurrentTimeLong - tracker.BleedoutStartTime;

        if (elapsed >= (ulong)_rules.BleedoutDurationMs)
        {
            TransitionToDead(character);
            return;
        }

        if (tracker.LastBleedoutDamage == 0 ||
            character.Shard.CurrentTimeLong - tracker.LastBleedoutDamage >= (ulong)_rules.BleedoutDamageRateMs)
        {
            tracker.LastBleedoutDamage = character.Shard.CurrentTimeLong;
            character.SetCurrentHealth(character.CurrentHealth - _rules.BleedoutDamageAmount);
        }
    }

    private sealed class LifecycleTracker
    {
        public CharacterLifecycleState State;
        public ulong BleedoutStartTime;
        public ulong DeadStartTime;
        public ulong LastBleedoutDamage;
    }
}
