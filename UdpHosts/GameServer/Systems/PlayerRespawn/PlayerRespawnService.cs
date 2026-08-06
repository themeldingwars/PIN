using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.GSS.V66.Character;
using GameServer.Entities.Character;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.PlayerRespawn;

public class PlayerRespawnService
{
    private static readonly ILogger Logger = Log.ForContext<PlayerRespawnService>();

    private readonly IShard _shard;
    private readonly IEventBus _eventBus;
    private readonly IPlayerRespawnRules _rules;
    private readonly CharacterLifecycleService _characterLifecycle;
    private readonly IDictionary<ulong, ulong> _respawnAt = new ConcurrentDictionary<ulong, ulong>();

    public PlayerRespawnService(IShard shard, IEventBus eventBus, IPlayerRespawnRules rules, CharacterLifecycleService characterLifecycle)
    {
        _shard = shard;
        _eventBus = eventBus;
        _rules = rules;
        _characterLifecycle = characterLifecycle;

        _eventBus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
        _eventBus.Subscribe<CharacterEnteredBleedoutEvent>(OnCharacterEnteredBleedout);
    }

    public void ForceRespawn(CharacterEntity character)
    {
        if (character.IsPlayerControlled && character.Player != null)
        {
            _respawnAt.Remove(character.EntityId);
            _characterLifecycle.Reset(character);
            character.SetCharacterState(CharacterStateData.CharacterStatus.Respawning, character.Shard.CurrentTime);
            character.Player.Respawn();
        }
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        foreach (var kvp in _respawnAt)
        {
            if (!_shard.Entities.TryGetValue(kvp.Key, out var entity) || entity is not CharacterEntity character)
            {
                _respawnAt.Remove(kvp.Key);
                continue;
            }

            if (_characterLifecycle.GetState(character) != CharacterLifecycleState.Dead)
            {
                _respawnAt.Remove(kvp.Key);
                continue;
            }

            if (currentTime >= kvp.Value)
            {
                if (character.IsPlayerControlled && character.Player != null)
                {
                    _respawnAt.Remove(kvp.Key);
                    _characterLifecycle.Reset(character);
                    character.SetCharacterState(CharacterStateData.CharacterStatus.Respawning, character.Shard.CurrentTime);
                    character.Player.Respawn();
                    _eventBus.Publish(new PlayerRespawnedEvent(character));
                }
            }
        }
    }

    private void OnCharacterDied(CharacterDiedEvent evt)
    {
        var character = evt.Target;
        if (!character.IsPlayerControlled || character.Player == null)
        {
            return;
        }

        ulong now = character.Shard.CurrentTimeLong;
        _respawnAt[character.EntityId] = now + (ulong)_rules.DeadDurationMs;

        character.SetRespawnTimes(new()
        {
            ForcedAt = _rules.RespawnEnabled ? (uint)(now + (ulong)_rules.DeadDurationMs) : 0,
            AvailableAt = 0,
        });
    }

    private void OnCharacterEnteredBleedout(CharacterEnteredBleedoutEvent evt)
    {
        var character = evt.Target;
        if (!character.IsPlayerControlled || character.Player == null)
        {
            return;
        }

        character.SetRespawnTimes(new()
        {
            ForcedAt = _rules.RespawnEnabled ? (uint)evt.BleedoutExpiryAt : 0,
            AvailableAt = character.Shard.CurrentTime + (uint)_rules.AllowRespawnAfterMs,
        });
    }
}
