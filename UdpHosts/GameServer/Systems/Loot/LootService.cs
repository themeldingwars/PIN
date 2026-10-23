using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.AreaVisualData.Event;
using AeroMessages.GSS.AreaVisualData.View;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.AreaVisualData;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Turret;
using GameServer.Entities.Vehicle;
using GameServer.Enums;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Loot;

/// <summary>
/// Rolls the loot tables of dying monsters and destroyed deployables to create LootObjects.
/// </summary>
public class LootService
{
    private const ulong _tickIntervalMs = 500;

    private static readonly ILogger _logger = Log.ForContext<LootService>();

    private readonly IShard _shard;
    private readonly ILootRules _rules;
    private readonly Random _rng = new();

    private readonly ConcurrentDictionary<ulong, AreaVisualDataEntity> _containers = new();
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<uint, LootDropInfo>> _drops = new();

    private ulong _lastTick;

    public LootService(IShard shard, IEventBus eventBus, ILootRules rules)
    {
        _shard = shard;
        _rules = rules;

        eventBus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
        eventBus.Subscribe<DeployableDestroyedEvent>(OnDeployableDestroyed);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime < _lastTick + _tickIntervalMs)
        {
            return;
        }

        _lastTick = currentTime;

        foreach ((ulong containerId, ConcurrentDictionary<uint, LootDropInfo> drops) in _drops)
        {
            if (!_shard.Entities.TryGetValue(containerId, out var entity) || entity is not AreaVisualDataEntity container)
            {
                _drops.TryRemove(containerId, out _);
                _containers.TryRemove(containerId, out _);
                continue;
            }

            foreach ((uint index, LootDropInfo drop) in drops)
            {
                if (currentTime > drop.DespawnAt)
                {
                    drops.TryRemove(index, out _);
                    container.ClearLootObject(index);
                }
            }

            if (drops.IsEmpty)
            {
                _drops.TryRemove(containerId, out _);
                _containers.TryRemove(containerId, out _);
            }
        }
    }

    /// <summary>
    /// Rolls one or two loot tables and drops the result at origin. level is the level of the entity that
    /// died, which is what the level_range gates in the tables are matched against.
    /// </summary>
    /// <param name="lootTableId">The primary loot table to roll.</param>
    /// <param name="secondLootTableId">The secondary loot table to roll, or 0 for none.</param>
    /// <param name="level">The level of the entity that died, matched against the tables' level_range gates.</param>
    /// <param name="origin">The world position to drop the loot at.</param>
    /// <param name="owner">The entity credited with the drop, or null.</param>
    public void DropLoot(uint lootTableId, uint secondLootTableId, int level, Vector3 origin, IEntity owner = null)
    {
        var context = new LootRollContext { Level = level, RequiredLevelWindow = _rules.RequiredLevelWindow };
        var results = new List<LootRollResult>();

        if (lootTableId != 0)
        {
            results.AddRange(LootRoller.Roll(lootTableId, context, _rng));
        }

        if (secondLootTableId != 0)
        {
            results.AddRange(LootRoller.Roll(secondLootTableId, context, _rng));
        }

        if (results.Count == 0)
        {
            return;
        }

        SpawnDrops(results, origin, owner);
    }

    /// <summary>
    /// Handles a CollectLoot command: clears the slot, tells scoped clients about it and hands the item over.
    /// lootObjectView is the EntityId the client sent, which carries the controller id in its low byte.
    /// </summary>
    /// <param name="client">The client that issued the collect command.</param>
    /// <param name="player">The player collecting the loot.</param>
    /// <param name="lootObjectView">The EntityId the client sent, carrying the controller id in its low byte.</param>
    /// <param name="lootIndex">The slot index within the loot object being collected.</param>
    /// <param name="requestedSdbId">The item id the client expects to collect.</param>
    /// <returns>True when the loot was collected successfully; false otherwise.</returns>
    public bool CollectLoot(INetworkClient client, IPlayer player, EntityId lootObjectView, uint lootIndex, uint requestedSdbId)
    {
        var character = player?.CharacterEntity;

        if (character == null)
        {
            return false;
        }

        var containerEntityId = lootObjectView.Backing & 0xFFFFFFFFFFFFFF00;

        _logger.Debug(
            "{Player} collects loot view {Backing:X} (controller {Controller}) container {ContainerId:X}[{Index}] for item {RequestedSdbId}",
            character,
            lootObjectView.Backing,
            lootObjectView.ControllerId,
            containerEntityId,
            lootIndex,
            requestedSdbId);

        if (!_drops.TryGetValue(containerEntityId, out var drops))
        {
            _logger.Debug(
                "{Player} collected from a container that is not tracked: {ContainerId:X}. {Known}",
                character,
                containerEntityId,
                DescribeTrackedContainers(character));

            return false;
        }

        if (!drops.TryRemove(lootIndex, out var drop))
        {
            _logger.Debug(
                "{Player} collected unknown loot slot {ContainerId:X}[{Index}], the container holds {Slots}",
                character,
                containerEntityId,
                lootIndex,
                DescribeSlots(drops));

            return false;
        }

        if (drop.SdbId != requestedSdbId)
        {
            _logger.Debug(
                "{Player} collected loot slot {ContainerId:X}[{Index}] as {RequestedSdbId}, it holds {SdbId}",
                character,
                containerEntityId,
                lootIndex,
                requestedSdbId,
                drop.SdbId);
        }

        var distance = Vector3.Distance(character.Position, drop.Position);

        if (distance > _rules.CollectRange)
        {
            _logger.Debug(
                "{Player} tried to collect loot slot {ContainerId:X}[{Index}] from {Distance:F1}m, the limit is {Range:F1}m",
                character,
                containerEntityId,
                lootIndex,
                distance,
                _rules.CollectRange);

            _drops.GetOrAdd(containerEntityId, _ => new ConcurrentDictionary<uint, LootDropInfo>())[lootIndex] = drop;

            return false;
        }

        if (_shard.Entities.TryGetValue(containerEntityId, out var entity) && entity is AreaVisualDataEntity container)
        {
            // The collected event first, then the slot is nulled so the view delta that follows it is the
            // last thing the client sees. Reliable channel: the view change is reliable too, and a lost
            // event would leave the client animating loot that has already been cleared.
            _shard.EntityMan.SendToScopedReliable(container, new LootObjectCollected
            {
                LootIndex = lootIndex,
                LootedByEntity = character.AeroEntityId,
                LootedToEntity = character.AeroEntityId,
            });

            container.ClearLootObject(lootIndex);

            var scopedIn = client is INetworkPlayer networkPlayer && _shard.EntityMan.HasScopedInEntity(containerEntityId, networkPlayer);

            _logger.Debug(
                "{Player} cleared loot slot {ContainerId:X}[{Index}], remaining {Slots}, collector scoped in: {Scoped}",
                character,
                containerEntityId,
                lootIndex,
                DescribeSlots(drops),
                scopedIn);
        }
        else
        {
            _logger.Debug(
                "{Player} collected loot slot {ContainerId:X}[{Index}] but its container entity is gone",
                character,
                containerEntityId,
                lootIndex);
        }

        var granted = GrantItem(client, player, drop.SdbId, drop.Quantity);

        _logger.Debug(
            "{Player} collected {Quantity}x item {SdbId} from {ContainerId:X}[{Index}], granted: {Granted}",
            character,
            drop.Quantity,
            drop.SdbId,
            containerEntityId,
            lootIndex,
            granted);

        if (drops.IsEmpty)
        {
            _drops.TryRemove(containerEntityId, out _);
            _containers.TryRemove(containerEntityId, out _);
        }

        return true;
    }

    /// <summary>
    /// Everything the server currently has on the ground, for the lootdump admin command.
    /// </summary>
    /// <param name="origin">The position to report coordinates/distances relative to.</param>
    /// <returns>A text listing of every drop currently on the ground.</returns>
    public string DumpLoot(Vector3 origin)
    {
        if (_drops.IsEmpty)
        {
            return "no loot is tracked";
        }

        return string.Join(
            " | ",
            _drops.Select(pair =>
            {
                var tracked = _shard.Entities.TryGetValue(pair.Key, out var entity);

                return $"container {pair.Key:X}{(tracked ? $"({Vector3.Distance(origin, entity.Position):F1}m)" : " (entity gone)")}{DescribeSlots(pair.Value)}";
            }));
    }

    private static bool IsPlayerAttributable(IEntity source)
    {
        return source switch
        {
            CharacterEntity character => character.IsPlayerControlled,
            VehicleEntity vehicle => vehicle.IsPlayerControlled,
            TurretEntity turret => turret.IsPlayerControlled,
            DeployableEntity deployable => deployable.IsPlayerOwned || deployable.Owner?.IsPlayerControlled == true,
            _ => false,
        };
    }

    private bool GrantItem(INetworkClient client, IPlayer player, uint sdbId, uint quantity)
    {
        var rootItem = SDBInterface.GetRootItem(sdbId);

        if (rootItem == null)
        {
            _logger.Debug("Loot item {SdbId} is not a RootItem.sdb_id, nothing was granted", sdbId);
            return false;
        }

        bool granted;

        if ((ItemType)rootItem.Type == ItemType.Powerup)
        {
            granted = ActivatePowerupAbility(client, player, sdbId);
        }
        else if (((ItemFlags)rootItem.Flags).HasFlag(ItemFlags.Resource))
        {
            player.Inventory.AddResource(sdbId, quantity);
            granted = true;
        }
        else
        {
            granted = false;

            for (var count = 0; count < quantity; count++)
            {
                granted |= player.Inventory.CreateItem(sdbId) != 0;
            }
        }

        // SimulateLootPickup is only the UI effect: the item flying to the hud of the collector.
        var pickup = new SimulateLootPickup
        {
            Item = new RewardInfoData
            {
                SdbId = sdbId,
                Quantity = (ushort)Math.Min(quantity, ushort.MaxValue),
            },
            RewardType = SimulateLootPickup.Type.General,
            HaveLootedBy = 1,
            LootedBy = player.CharacterEntity.AeroEntityId,
        };

        client.NetChannels[ChannelType.ReliableGss].SendMessage(pickup, player.CharacterEntity.EntityId);

        return granted;
    }

    private bool ActivatePowerupAbility(INetworkClient client, IPlayer player, uint sdbId)
    {
        var abilityModule = SDBInterface.GetAbilityModule(sdbId);

        if (abilityModule == null || abilityModule.AbilityChainId == 0)
        {
            _logger.Debug("Collected powerup {SdbId} has no AbilityModule ability to trigger", sdbId);
            return false;
        }

        var abilityId = abilityModule.AbilityChainId;
        var character = player.CharacterEntity;
        var activationTime = _shard.CurrentTime;

        if (character.IsPlayerControlled)
        {
            client.NetChannels[ChannelType.ReliableGss].SendMessage(
                new AbilityActivated
                {
                    ActivatedAbilityId = abilityId,
                    ActivatedTime = activationTime,
                    AbilityCooldownsData = new AbilityCooldownsData
                    {
                        ActiveCooldowns_Group1 = [],
                        ActiveCooldowns_Group2 = [],
                        GlobalCooldown_Activated_Time = activationTime,
                        GlobalCooldown_ReadyAgain_Time = activationTime + 300,
                        GlobalCooldown_Flags = 0,
                    },
                },
                character.EntityId);
        }

        _shard.Abilities.HandleActivateAbility(_shard, character, abilityId, activationTime, new AptitudeTargets());

        _logger.Debug("Collected powerup {SdbId} triggered ability {AbilityId} for {Player}", sdbId, abilityId, character);

        return true;
    }

    private string DescribeSlots(ConcurrentDictionary<uint, LootDropInfo> drops)
    {
        if (drops.IsEmpty)
        {
            return "nothing";
        }

        return string.Join(", ", drops.Select(pair => $"[{pair.Key}]={pair.Value.SdbId}x{pair.Value.Quantity}@{pair.Value.Position}"));
    }

    private string DescribeTrackedContainers(CharacterEntity character)
    {
        if (_drops.IsEmpty)
        {
            return "no loot is tracked at all";
        }

        var position = character?.Position ?? Vector3.Zero;

        return "tracked: " + string.Join(", ", _drops.Select(pair =>
        {
            var distance = _shard.Entities.TryGetValue(pair.Key, out var entity)
                ? Vector3.Distance(position, entity.Position)
                : -1f;

            return $"{pair.Key:X}({distance:F1}m){DescribeSlots(pair.Value)}";
        }));
    }

    private void OnCharacterDied(CharacterDiedEvent evt)
    {
        var character = evt.Target;
        var killer = evt.Source;

        if (character.IsPlayerControlled)
        {
            return;
        }

        var monster = SDBInterface.GetMonster(character.StaticInfo.CharacterTypeId);

        if (monster == null)
        {
            _logger.Debug("No Monster row for {Character}, cannot roll loot", character);
            return;
        }

        if (monster.LootTableId == 0 && monster.LootTable2Id == 0)
        {
            return;
        }

        if (_rules.RequirePlayerKill && !IsPlayerAttributable(killer))
        {
            return;
        }

        DropLoot(monster.LootTableId, monster.LootTable2Id, character.Level, character.Position, killer);
    }

    private void OnDeployableDestroyed(DeployableDestroyedEvent evt)
    {
        var killer = evt.Source;

        var deployable = SDBInterface.GetDeployable(evt.Target.Type);

        if (deployable == null || deployable.LootTableId == 0)
        {
            return;
        }

        if (_rules.RequirePlayerKill && !IsPlayerAttributable(killer))
        {
            return;
        }

        DropLoot(deployable.LootTableId, 0, (int)deployable.DeployableLevel, evt.Target.Position, killer);
    }

    private void SpawnDrops(List<LootRollResult> results, Vector3 origin, IEntity owner)
    {
        var now = _shard.CurrentTimeLong;
        var container = GetContainer(origin);
        var drops = _drops.GetOrAdd(container.EntityId, _ => new ConcurrentDictionary<uint, LootDropInfo>());

        foreach (var result in results)
        {
            var index = container.GetFreeLootIndex();

            if (index == AreaVisualDataEntity.InvalidIndex)
            {
                container = CreateContainer(origin);
                drops = _drops.GetOrAdd(container.EntityId, _ => new ConcurrentDictionary<uint, LootDropInfo>());
                index = container.GetFreeLootIndex();
            }

            var position = GetDropPosition(origin);

            drops[index] = new LootDropInfo(result.SdbId, result.Quantity, position, now + (ulong)_rules.LootLingerMs);
            _shard.EntityMan.SetRemainingLifetime(container, (uint)_rules.LootLingerMs);

            container.SetLootObject(index, new LootObjectData
            {
                Time = _shard.CurrentTime,
                HaveEntity = 0,
                HaveFaction = 0,
                OriginOffset = origin - position,
                Position = position,
                LootSdbId = result.SdbId,
                Quantity = (byte)Math.Min(result.Quantity, byte.MaxValue),
                Unk5 = 0,
                Unk6 = 0,
                ItemModules = [0, 0],
            });
        }

        _logger.Debug(
            "Dropped {Count} loot stack(s) around {Origin} from {Owner}: {Slots}",
            results.Count,
            origin,
            owner,
            string.Join(", ", _drops.Select(pair => $"{pair.Key:X}{DescribeSlots(pair.Value)}")));
    }

    private AreaVisualDataEntity GetContainer(Vector3 origin)
    {
        AreaVisualDataEntity closest = null;
        var closestDistance = float.MaxValue;
        var rangeSquared = _rules.ContainerRange * _rules.ContainerRange;

        foreach (var container in _containers.Values)
        {
            if (container.GetFreeLootIndex() == AreaVisualDataEntity.InvalidIndex)
            {
                continue;
            }

            var distance = Vector3.DistanceSquared(origin, container.Position);

            if (distance <= rangeSquared && distance < closestDistance)
            {
                closest = container;
                closestDistance = distance;
            }
        }

        return closest ?? CreateContainer(origin);
    }

    private AreaVisualDataEntity CreateContainer(Vector3 origin)
    {
        var container = _shard.EntityMan.SpawnAreaVisualData(origin, new ScopingComponent { Range = _rules.ContainerRange });
        _containers[container.EntityId] = container;

        return container;
    }

    private Vector3 GetDropPosition(Vector3 origin)
    {
        var angle = _rng.NextDouble() * Math.Tau;
        var distance = _rng.NextDouble() * _rules.DropRadius;

        var position = origin + new Vector3(
            (float)(Math.Cos(angle) * distance),
            (float)(Math.Sin(angle) * distance),
            0.5f);

        var ground = _shard.Physics.SampleGroundHeight(position);

        if (ground.HasValue)
        {
            position.Z = ground.Value + 0.5f;
        }

        return position;
    }
}
