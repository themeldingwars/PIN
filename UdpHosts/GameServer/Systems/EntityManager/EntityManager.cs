using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Aero.Gen;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Melding.View;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities;
using GameServer.Entities.AreaVisualData;
using GameServer.Entities.Carryable;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Melding;
using GameServer.Entities.MeldingBubble;
using GameServer.Entities.Outpost;
using GameServer.Entities.Thumper;
using GameServer.Entities.Turret;
using GameServer.Entities.Vehicle;
using GameServer.Extensions;
using Timer = System.Threading.Timer;

namespace GameServer;

public class EntityManager
{
    private const byte ServerId = 31;
    private Shard Shard;
    private uint Counter = 0;

    private ulong LastUpdateFlush = 0;
    private ulong UpdateFlushIntervalMs = 5;
    private ulong LastScopeIn = 0;
    private ulong ScopeInIntervalMs = 20;
    private ulong LastScopeCheck = 0;
    private ulong ScopeCheckIntervalMs = 5000;
    private ulong LastLifetimeCheck = 0;
    private ulong LifetimeCheckIntervalMs = 1000;
    private bool hasSpawnedTestEntities = false;

    private ConcurrentDictionary<ulong, HashSet<INetworkPlayer>> ScopedPlayersByEntity = new ConcurrentDictionary<ulong, HashSet<INetworkPlayer>>();
    
    private ConcurrentQueue<ScopeInRequest> QueuedScopeIn = new ConcurrentQueue<ScopeInRequest>();
    private ConcurrentDictionary<ulong, Lifetime> LifetimeByEntity = new ConcurrentDictionary<ulong, Lifetime>();

    public EntityManager(Shard shard)
    {
        Shard = shard;
    }

    public int GetNumberOfScopedEntities(IPlayer player)
    {
        return ScopedPlayersByEntity.Values.Count(set => set.Contains(player));
    }

    public bool HasScopedInEntity(ulong entityId, INetworkPlayer player)
    {
        return ScopedPlayersByEntity.TryGetValue(entityId, out var players) && players.Contains(player);
    }

    public CharacterEntity SpawnCharacter(uint typeId, Vector3 position, CharacterEntity owner = null)
    {
        var characterEntity = new CharacterEntity(Shard, Shard.GetNextGuid(), owner);
        characterEntity.LoadMonster(typeId);
        characterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Living, Shard.CurrentTime);
        characterEntity.SetPosition(position);
        characterEntity.SetSpawnPose();
        Add(characterEntity.EntityId, characterEntity);
        return characterEntity;
    }

    public VehicleEntity SpawnVehicle(ushort typeId, Vector3 position, Quaternion orientation, CharacterEntity owner, bool autoMount = false)
    {
        var vehicleInfo = SDBUtils.GetDetailedVehicleInfo(typeId);
        var vehicleEntity = new VehicleEntity(Shard, Shard.GetNextGuid(), owner);
        vehicleEntity.Position = position;
        vehicleEntity.Load(vehicleInfo);
        position.Z += vehicleInfo.SpawnHeight;
        vehicleEntity.SetSpawnPose(new AeroMessages.GSS.V66.Vehicle.Controller.SpawnPoseData()
        {
            Position = position,
            Rotation = orientation,
            Direction = vehicleEntity.AimDirection,
            Time = Shard.CurrentTime,
        });
        vehicleEntity.SetPoseData(new AeroMessages.GSS.V66.Vehicle.Command.MovementInput()
        {
            Position = position,
            Rotation = orientation,
            Direction = vehicleEntity.AimDirection,
            MovementState = 0x1000,
            Time = Shard.CurrentTime,
        });
        vehicleEntity.Scoping = new ScopingComponent() { Range = vehicleInfo.ScopeRange };
        if (owner is { IsPlayerControlled: true })
        {
            vehicleEntity.SetOwningPlayer(owner.Player);
        }

        Add(vehicleEntity.EntityId, vehicleEntity);

        if (owner != null && autoMount)
        {
            vehicleEntity.AddOccupant(owner);
        }

        if (vehicleEntity.SpawnAbility != 0)
        {
            Shard.Abilities.HandleActivateAbility(Shard, vehicleEntity, vehicleEntity.SpawnAbility);
        }

        return vehicleEntity;
    }

    public DeployableEntity SpawnDeployable(uint typeId, Vector3 position, Quaternion orientation, CharacterEntity owner = null)
    {
        var deployableInfo = SDBInterface.GetDeployable(typeId);
        var deployableEntity = new DeployableEntity(Shard, Shard.GetNextGuid(), typeId, 0, owner);
        var aimDirection = new Vector3(deployableInfo.AimDirection.x, deployableInfo.AimDirection.y, deployableInfo.AimDirection.z);
        deployableEntity.SetPosition(position);
        deployableEntity.SetOrientation(orientation);
        deployableEntity.SetAimDirection(aimDirection);
        deployableEntity.Scale = deployableInfo.Scale;

        if (deployableInfo.InteractionType != 0)
        {
            deployableEntity.Interaction = new InteractionComponent()
            {
                Radius = deployableInfo.InteractRadius,
                Height = deployableInfo.InteractHeight,
                CompletedAbilityId = deployableInfo.InteractCompletedAbilityid,
                StartedAbilityId = deployableInfo.InteractAbilityid,
                DurationMs = deployableInfo.InteractionDurationMs,
                Type = (InteractionType)deployableInfo.InteractionType
            };
        }

        if (deployableInfo.ScopeRange != 0)
        {
            // TODO: What does ScopeRange 0 mean? Anyway, it will get a default from the component if so.
            deployableEntity.Scoping = new ScopingComponent() { Range = deployableInfo.ScopeRange };
        }

        Add(deployableEntity.EntityId, deployableEntity);

        if (deployableInfo.SpawnAbilityid != 0)
        {
            Shard.Abilities.HandleActivateAbility(Shard, deployableEntity, deployableInfo.SpawnAbilityid);
        }

        if (deployableInfo.ConstructedAbilityid != 0)
        {
            var timer = new Timer(state =>
                 {
                     Shard.Abilities.HandleActivateAbility(Shard, deployableEntity, deployableInfo.ConstructedAbilityid);

                     ((Timer)state)?.Dispose();
                 });
            timer.Change(deployableInfo.BuildTimeMs, Timeout.Infinite);
        }

        if (deployableInfo.TurretType != 0)
        {
            deployableEntity.Turret = SpawnTurret(deployableInfo.TurretType, deployableEntity);
        }

        if (deployableInfo.PoweredOnAbility != 0)
        {
            deployableEntity.PoweredOnAbility = deployableInfo.PoweredOnAbility;
            var poweredOnAbility = deployableInfo.PoweredOnAbility;

            if (deployableEntity.Turret != null)
            {
                if (poweredOnAbility == 34214)
                {
                    // Fixes AA and Heavy Flak Turrets, taken from Anti-Armor Turret id 3823
                    poweredOnAbility = 40497;
                }
            }

            var timer = new Timer(state =>
                 {
                     Console.WriteLine($"deployable: Executing ability {deployableInfo.PoweredOnAbility}");
                     Shard.Abilities.HandleActivateAbility(Shard, deployableEntity, poweredOnAbility);

                     ((Timer)state)?.Dispose();
                 });
            timer.Change(deployableInfo.BuildTimeMs, Timeout.Infinite);
        }

        if (deployableInfo.PoweredOffAbility != 0)
        {
            deployableEntity.PoweredOffAbility = deployableInfo.PoweredOffAbility;
        }

        return deployableEntity;
    }

    public TurretEntity SpawnTurret(uint typeId, BaseEntity parent, byte parentChildIndex = 0, byte posture = 0)
    {
        if (posture == 0)
        {
            posture = SDBInterface.GetTurret(typeId).Posture;
        }

        var turretEntity = new TurretEntity(Shard, Shard.GetNextGuid(), typeId, parent, parentChildIndex, posture);

        Add(turretEntity.EntityId, turretEntity);

        return turretEntity;
    }

    public MeldingEntity SpawnMelding(string perimiterSetName, ActiveDataStruct activeData)
    {
        var meldingEntity = new MeldingEntity(Shard, Shard.GetNextGuid(), perimiterSetName);
        meldingEntity.SetActiveData(activeData);
        Add(meldingEntity.EntityId, meldingEntity);
        return meldingEntity;
    }

    public AreaVisualDataEntity SpawnAreaVisualData(Vector3 position, ScopingComponent scoping)
    {
        var areaVisualData = new AreaVisualDataEntity(Shard, Shard.GetNextGuid())
            {
                Scoping = scoping, Position = position,
            };
        Add(areaVisualData.EntityId, areaVisualData);
        return areaVisualData;
    }

    public OutpostEntity SpawnOutpost(Outpost outpost)
    {
        var outpostEntity = new OutpostEntity(Shard, Shard.GetNextGuid(), outpost);
        Add(outpostEntity.EntityId, outpostEntity);

        if (!Shard.Outposts.TryGetValue(outpost.ZoneId, out var zoneOutposts))
        {
            zoneOutposts = new ConcurrentDictionary<uint, OutpostEntity>();
            Shard.Outposts[outpost.ZoneId] = zoneOutposts;
        }

        zoneOutposts[outpost.Id] = outpostEntity;
        return outpostEntity;
    }

    public ThumperEntity SpawnThumper(
        uint nodeType,
        Vector3 position,
        CharacterEntity owner,
        ResourceNodeBeaconCalldownCommandDef commandDef)
    {
        var beacon = SDBInterface.GetResourceNodeBeacon(commandDef.ResourceNodeBeaconId);
        var thumperEntity = new ThumperEntity(Shard, Shard.GetNextGuid(), nodeType, position, owner, commandDef);
        thumperEntity.Scale = beacon.Scale;
        Add(thumperEntity.EntityId, thumperEntity);
        return thumperEntity;
    }

    public CarryableEntity SpawnCarryable(uint type, Vector3 position)
    {
        var carryableEntity = new CarryableEntity(Shard, Shard.GetNextGuid(), type);
        carryableEntity.SetPosition(position);
        Add(carryableEntity.EntityId, carryableEntity);
        return carryableEntity;
    }

    public void TempSpawnTestEntities()
    {
        // Aero
        var aero = SpawnCharacter(356, new Vector3(167.84642f, 262.20822f, 491.86758f));

        // Battleframe Station
        SpawnDeployable(395, new Vector3(170.84642f, 243.20822f, 491.71597f), new Quaternion(0f, 0f, 0.92874485f, 0.37071964f));

        // Thumper
        Shard.EncounterMan.CreateThumper(20, new Vector3(158.3f, 249.3f, 491.93f), aero, SDBInterface.GetResourceNodeBeaconCalldownCommandDef(766269));

        // Datapad
        SpawnCarryable(26, new Vector3(160.3f, 250.3f, 491.93f));
    }

    public void SpawnZoneEntities(uint zoneId)
    {
        // Deployable
        foreach (var entry in CustomDBInterface.GetZoneDeployables(zoneId))
        {
            var deployable = entry.Value;
            SpawnDeployable(deployable.Type, deployable.Position, deployable.Orientation);
        }

        // Melding
        foreach (var entry in CustomDBInterface.GetZoneMeldings(zoneId))
        {
            var melding = entry.Value;
            SpawnMelding(melding.PerimiterSetName, new ActiveDataStruct()
            {
                TimestampMicro = melding.Unk1,
                Unk2 = melding.Unk2,
                Unk3 = melding.Unk3,
                FromPoints = melding.ControlPoints,
                FromTangents = melding.Offsets,
                ToPoints = melding.ControlPoints,
                ToTangets = melding.Offsets,
            });
        }

        // Outpost
        foreach (var entry in CustomDBInterface.GetZoneOutposts(zoneId))
        {
            var outpost = entry.Value;
            SpawnOutpost(outpost);
        }
    }

    public void SetRemainingLifetime(IEntity entity, uint timeMs)
    {
        var tracker = LifetimeByEntity.TryGetValue(entity.EntityId, out var value) ? value : new Lifetime();

        tracker.ExpireAt = Shard.CurrentTimeLong + timeMs;
        LifetimeByEntity[entity.EntityId] = tracker;
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Spawn test entities on first real tick
        if (!hasSpawnedTestEntities && currentTime != 0)
        {
            hasSpawnedTestEntities = true;

            if (Shard.Settings.LoadZoneEntities)
            {
                SpawnZoneEntities(Shard.ZoneId);

                // TODO: Remove these in favor of using the files instead
                if (Shard.ZoneId == 448)
                {
                    TempSpawnTestEntities();
                }
            }
        }

        // Process queued scope-ins
        if (!QueuedScopeIn.IsEmpty && currentTime > LastScopeIn + ScopeInIntervalMs)
        {
            bool ok = QueuedScopeIn.TryDequeue(out ScopeInRequest request);
            if (ok)
            {
                ScopeIn(request.Player, request.Entity);
            }

            LastScopeIn = currentTime;
        }

        // Flush changes periodically
        if (currentTime > LastUpdateFlush + UpdateFlushIntervalMs)
        {
            LastUpdateFlush = currentTime;
            foreach (var entity in Shard.Entities.Values)
            {
                FlushChanges(entity);
            }
        }

        // Check if any entities have ran out lifetime
        if (currentTime > LastLifetimeCheck + LifetimeCheckIntervalMs)
        {
            LastLifetimeCheck = currentTime;
            foreach ((ulong entityId, Lifetime tracker) in LifetimeByEntity)
            {
                if (currentTime > tracker.ExpireAt)
                {
                    Remove(entityId);
                }
            }
        }

        // Check if we should scope in/out entities for each player
        if (currentTime > LastScopeCheck + ScopeCheckIntervalMs)
        {
            LastScopeCheck = currentTime;
            var players = Shard.Clients.Values.Where((client) => client.CanReceiveGSS);
            var entities = Shard.Entities.Values;

            foreach (var entity in entities)
            {
                float distanceThreshold = entity.GetScopeRange();
                var currentlyScoped = ScopedPlayersByEntity[entity.EntityId];
                var entityPosition = entity.Position;
                foreach (var player in players)
                {
                    if (player.CharacterEntity == null)
                    {
                        // If we somehow don't have a CharacterEntity, we can't check positions, so why bother scoping in entities.
                        continue;
                    }

                    bool isScoped = currentlyScoped.Contains(player);
                    bool shouldBeScoped = false;

                    // Determine shouldBeScoped
                    if (entity == player.CharacterEntity)
                    {
                        // Players local character should probably always be scoped in
                        shouldBeScoped = true;
                    }
                    else if (entity == player.CharacterEntity.AttachedToEntity)
                    {
                        // If players local character is attached to something, don't scope that out
                        shouldBeScoped = true;
                    }
                    else if (entity.IsGlobalScope())
                    {
                        shouldBeScoped = true;
                    }
                    else
                    {
                        var playerPosition = player.CharacterEntity.Position;
                        float distance = Vector3.Distance(entityPosition, playerPosition);
                        shouldBeScoped = distance <= distanceThreshold;
                    }

                    // Resolve shouldBeScoped
                    if (isScoped != shouldBeScoped)
                    {
                        if (shouldBeScoped)
                        {
                            QueuedScopeIn.Enqueue(new ScopeInRequest { Player = player, Entity = entity });
                        }
                        else
                        {
                            ScopeOut(player, entity);
                        }
                    }
                }
            }
        }
    }

    public void Add(ulong guid, IEntity entity)
    {
        ScopedPlayersByEntity.TryAdd(guid, new());
        Shard.Entities.Add(guid, entity);
        OnAddedEntity(entity);
    }

    public void Add(IEntity entity)
    {
        var guid = new Core.Data.EntityGuid(ServerId, Shard.CurrentTime, Counter++, (byte)Enums.GSS.Controllers.Character);
        ScopedPlayersByEntity.TryAdd(guid.Full, new());
        Shard.Entities.Add(guid.Full, entity);
        OnAddedEntity(entity);
    }

    public void Remove(IEntity entity)
    {
        Remove(entity.EntityId);
    }

    public void Remove(Core.Data.EntityGuid guid)
    {
        Remove(guid.Full);
    }

    public void Remove(ulong guid)
    {
        Shard.Entities.TryGetValue(guid, out IEntity entity);
        if (entity != null)
        {
            OnRemovedEntity(entity);
            Shard.Entities.Remove(guid);
            ScopedPlayersByEntity.TryRemove(guid, out var v);
        }
    }

    public void KeyframeRequest(INetworkClient client, IPlayer player, IEntity entity, Enums.GSS.Controllers typecode, uint clientChecksum)
    {
        switch (entity)
        {
            case CharacterEntity character:
                bool isCharacterController = character.IsPlayerControlled && character.Player == player;
                switch (typecode)
                {
                    case Enums.GSS.Controllers.Character_BaseController:
                        if (isCharacterController && character.Character_BaseController != null)
                        {
                            uint ourChecksum = character.Character_BaseController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(character.Character_BaseController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_MissionAndMarkerController:
                        if (isCharacterController && character.Character_MissionAndMarkerController != null)
                        {
                            uint ourChecksum = character.Character_MissionAndMarkerController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(character.Character_MissionAndMarkerController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_CombatController:
                        if (isCharacterController && character.Character_CombatController != null)
                        {
                            uint ourChecksum = character.Character_CombatController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(character.Character_CombatController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_LocalEffectsController:
                        if (isCharacterController && character.Character_LocalEffectsController != null)
                        {
                            uint ourChecksum = character.Character_LocalEffectsController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(character.Character_LocalEffectsController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_SpectatorController:
                        if (isCharacterController && character.Character_SpectatorController != null)
                        {
                            uint ourChecksum = character.Character_SpectatorController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(character.Character_SpectatorController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_ObserverView:
                        if (character.Character_ObserverView != null)
                        {
                            uint ourChecksum = character.Character_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(character.Character_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_EquipmentView:
                        if (character.Character_EquipmentView != null)
                        {
                            uint ourChecksum = character.Character_EquipmentView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(character.Character_EquipmentView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_CombatView:
                        if (character.Character_CombatView != null)
                        {
                            uint ourChecksum = character.Character_CombatView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(character.Character_CombatView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_MovementView:
                        if (character.Character_MovementView != null)
                        {
                            uint ourChecksum = character.Character_MovementView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(character.Character_MovementView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Character_TinyObjectView:
                        if (character.Character_TinyObjectView != null)
                        {
                            uint ourChecksum = character.Character_TinyObjectView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(character.Character_TinyObjectView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;

            case MeldingEntity melding:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.Melding_ObserverView:
                        if (melding.Melding_ObserverView != null)
                        {
                            uint ourChecksum = melding.Melding_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(melding.Melding_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case MeldingBubbleEntity meldingBubble:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.MeldingBubble_ObserverView:
                        if (meldingBubble.MeldingBubble_ObserverView != null)
                        {
                            uint ourChecksum = meldingBubble.MeldingBubble_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(meldingBubble.MeldingBubble_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case AreaVisualDataEntity areaVisualData:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.AreaVisualData_ObserverView:
                        if (areaVisualData.AreaVisualData_ObserverView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.AreaVisualData_ParticleEffectsView:
                        if (areaVisualData.AreaVisualData_ParticleEffectsView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_ParticleEffectsView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_ParticleEffectsView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.AreaVisualData_MapMarkerView:
                        if (areaVisualData.AreaVisualData_MapMarkerView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_MapMarkerView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_MapMarkerView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.AreaVisualData_TinyObjectView:
                        if (areaVisualData.AreaVisualData_TinyObjectView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_TinyObjectView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_TinyObjectView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.AreaVisualData_LootObjectView:
                        if (areaVisualData.AreaVisualData_LootObjectView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_LootObjectView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_LootObjectView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.AreaVisualData_ForceShieldView:
                        if (areaVisualData.AreaVisualData_ForceShieldView != null)
                        {
                            uint ourChecksum = areaVisualData.AreaVisualData_ForceShieldView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(areaVisualData.AreaVisualData_ForceShieldView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case VehicleEntity vehicle:
                bool isVehicleController = vehicle.IsPlayerControlled && vehicle.ControllingPlayer == player;
                switch (typecode)
                {
                    case Enums.GSS.Controllers.Vehicle_BaseController:
                        if (isVehicleController && vehicle.Vehicle_BaseController != null)
                        {
                            uint ourChecksum = vehicle.Vehicle_BaseController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(vehicle.Vehicle_BaseController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Vehicle_CombatController:
                        if (isVehicleController && vehicle.Vehicle_CombatController != null)
                        {
                            uint ourChecksum = vehicle.Vehicle_CombatController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(vehicle.Vehicle_CombatController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Vehicle_ObserverView:
                        if (vehicle.Vehicle_ObserverView != null)
                        {
                            uint ourChecksum = vehicle.Vehicle_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(vehicle.Vehicle_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Vehicle_CombatView:
                        if (vehicle.Vehicle_CombatView != null)
                        {
                            uint ourChecksum = vehicle.Vehicle_CombatView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(vehicle.Vehicle_CombatView, entity.EntityId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Vehicle_MovementView:
                        if (vehicle.Vehicle_MovementView != null)
                        {
                            uint ourChecksum = vehicle.Vehicle_MovementView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(vehicle.Vehicle_MovementView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case DeployableEntity deployable:
                switch (typecode)
                {
                    // TODO: Deployable_HardpointView
                    case Enums.GSS.Controllers.Deployable_ObserverView:
                        if (deployable.Deployable_ObserverView != null)
                        {
                            uint ourChecksum = deployable.Deployable_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(deployable.Deployable_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case TurretEntity turret:
                bool isTurretController = turret.IsPlayerControlled && turret.ControllingPlayer == player;
                switch (typecode)
                {
                    case Enums.GSS.Controllers.Turret_BaseController:
                        if (isTurretController && turret.Turret_BaseController != null)
                        {
                            uint ourChecksum = turret.Turret_BaseController.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(turret.Turret_BaseController, entity.EntityId, player.PlayerId);
                            }
                        }

                        break;
                    case Enums.GSS.Controllers.Turret_ObserverView:
                        if (turret.Turret_ObserverView != null)
                        {
                            uint ourChecksum = turret.Turret_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(turret.Turret_ObserverView, entity.EntityId);
                            }
                        }

                        break;

                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");

                        break;
                }

                break;
            case OutpostEntity outpost:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.Outpost_ObserverView:
                        if (outpost.Outpost_ObserverView != null)
                        {
                            uint ourChecksum = outpost.Outpost_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(outpost.Outpost_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case ThumperEntity thumper:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.ResourceNode_ObserverView:
                        if (thumper.ResourceNode_ObserverView != null)
                        {
                            uint ourChecksum = thumper.ResourceNode_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(thumper.ResourceNode_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            case CarryableEntity carryable:
                switch (typecode)
                {
                    case Enums.GSS.Controllers.CarryableObject_ObserverView:
                        if (carryable.CarryableObject_ObserverView != null)
                        {
                            uint ourChecksum = carryable.CarryableObject_ObserverView.SerializeToChecksum();
                            if (clientChecksum == ourChecksum)
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendChecksum(entity.EntityId, typecode, clientChecksum);
                            }
                            else
                            {
                                client.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(carryable.CarryableObject_ObserverView, entity.EntityId);
                            }
                        }

                        break;
                    default:
                        Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                        break;
                }

                break;
            default:
                Console.WriteLine($"Unhandled KeyframeRequest for {typecode}");
                break;
        }
    }

    public void ScopeIn(INetworkPlayer player, IEntity entity)
    {
        // Avoid sending messages if player is not in the appropriate state
        if (!player.CanReceiveGSS)
        {
            return;
        }

        ScopedPlayersByEntity[entity.EntityId].Add(player);

        if (entity is CharacterEntity character)
        {
            if (character.IsPlayerControlled && character.Player == player)
            {
                var baseController = character.Character_BaseController;
                var combatController = character.Character_CombatController;
                var missionController = character.Character_MissionAndMarkerController;
                var effectsController = character.Character_LocalEffectsController;
                var specController = character.Character_SpectatorController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;
                bool haveMissionController = missionController != null;
                bool haveEffectsController = effectsController != null;
                bool haveSpecController = specController != null;

                if (haveBaseController && haveCombatController && haveMissionController && haveEffectsController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(combatController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(effectsController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(missionController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendMessage(new CharacterLoaded(), entity.EntityId);
                }
            }

            var observer = character.Character_ObserverView;
            var equipment = character.Character_EquipmentView;
            var combat = character.Character_CombatView;
            var movement = character.Character_MovementView;
            var tinyobject = character.Character_TinyObjectView;

            bool haveObserver = observer != null;
            bool haveEquipment = equipment != null;
            bool haveCombat = combat != null;
            bool haveMovement = movement != null;
            bool haveTinyObject = tinyobject != null;

            if (haveObserver && haveEquipment && haveCombat && haveMovement)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(equipment, entity.EntityId);
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(combat, entity.EntityId);
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(movement, entity.EntityId);
            }

            if (haveTinyObject)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(tinyobject, entity.EntityId);
            }
        }
        else if (entity is MeldingEntity melding)
        {
            var observer = melding.Melding_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is MeldingBubbleEntity meldingBubble)
        {
            var observer = meldingBubble.MeldingBubble_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is VehicleEntity vehicle)
        {
            if (vehicle.IsPlayerControlled && vehicle.ControllingPlayer == player)
            {
                var baseController = vehicle.Vehicle_BaseController;
                var combatController = vehicle.Vehicle_CombatController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;

                if (haveBaseController && haveCombatController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(combatController, entity.EntityId, player.PlayerId);
                }
            }

            var observer = vehicle.Vehicle_ObserverView;
            var combat = vehicle.Vehicle_CombatView;
            var movement = vehicle.Vehicle_MovementView;

            bool haveObserver = observer != null;
            bool haveCombat = combat != null;
            bool haveMovement = movement != null;

            if (haveObserver && haveCombat && haveMovement)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(combat, entity.EntityId);
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(movement, entity.EntityId);
            }
        }
        else if (entity is DeployableEntity deployable)
        {
            var observer = deployable.Deployable_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is TurretEntity turret)
        {
            if (turret.IsPlayerControlled && turret.ControllingPlayer == player)
            {
                var baseController = turret.Turret_BaseController;

                bool haveBaseController = baseController != null;

                if (haveBaseController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerKeyframe(baseController, entity.EntityId, player.PlayerId);
                }
            }

            var observer = turret.Turret_ObserverView;

            bool haveObserver = observer != null;

            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is OutpostEntity outpost)
        {
            var observer = outpost.Outpost_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is ThumperEntity thumper)
        {
            var observer = thumper.ResourceNode_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is CarryableEntity carryable)
        {
            var observer = carryable.CarryableObject_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }
        }
        else if (entity is AreaVisualDataEntity avd)
        {
            var observer = avd.AreaVisualData_ObserverView;
            if (observer != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(observer, entity.EntityId);
            }

            var particleEffects = avd.AreaVisualData_ParticleEffectsView;
            if (particleEffects != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(particleEffects, entity.EntityId);
            }

            var mapMarker = avd.AreaVisualData_MapMarkerView;
            if (mapMarker != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(mapMarker, entity.EntityId);
            }

            var tinyObject = avd.AreaVisualData_TinyObjectView;
            if (tinyObject != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(tinyObject, entity.EntityId);
            }

            var lootObject = avd.AreaVisualData_LootObjectView;
            if (lootObject != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(lootObject, entity.EntityId);
            }

            var forceShield = avd.AreaVisualData_ForceShieldView;
            if (forceShield != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewKeyframe(forceShield, entity.EntityId);
            }
        }
    }

    public void ScopeOut(INetworkPlayer player, IEntity entity)
    {
        // Avoid sending messages if player is not in the appropriate state
        if (!player.CanReceiveGSS)
        {
            return;
        }

        ScopedPlayersByEntity[entity.EntityId].Remove(player);

        if (entity is CharacterEntity character)
        {
            if (character.IsPlayerControlled && character.Player == player)
            {
                var baseController = character.Character_BaseController;
                var combatController = character.Character_CombatController;
                var missionController = character.Character_MissionAndMarkerController;
                var effectsController = character.Character_LocalEffectsController;
                var specController = character.Character_SpectatorController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;
                bool haveMissionController = missionController != null;
                bool haveEffectsController = effectsController != null;
                bool haveSpecController = specController != null;

                if (haveBaseController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(baseController, entity.EntityId, player.PlayerId);
                }

                if (haveCombatController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(combatController, entity.EntityId, player.PlayerId);
                }

                if (haveMissionController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(missionController, entity.EntityId, player.PlayerId);
                }

                if (haveEffectsController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(effectsController, entity.EntityId, player.PlayerId);
                }

                if (haveSpecController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(specController, entity.EntityId, player.PlayerId);
                }
            }

            var observer = character.Character_ObserverView;
            var equipment = character.Character_EquipmentView;
            var combat = character.Character_CombatView;
            var movement = character.Character_MovementView;
            var tinyobject = character.Character_TinyObjectView;

            bool haveObserver = observer != null;
            bool haveEquipment = equipment != null;
            bool haveCombat = combat != null;
            bool haveMovement = movement != null;
            bool haveTinyObject = tinyobject != null;

            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }

            if (haveEquipment)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(equipment, entity.EntityId);
            }

            if (haveCombat)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(combat, entity.EntityId);
            }

            if (haveMovement)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(movement, entity.EntityId);
            }

            if (haveTinyObject)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(tinyobject, entity.EntityId);
            }
        }
        else if (entity is MeldingEntity melding)
        {
            var observer = melding.Melding_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is MeldingBubbleEntity meldingBubble)
        {
            var observer = meldingBubble.MeldingBubble_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is VehicleEntity vehicle)
        {
            if (vehicle.IsPlayerControlled && vehicle.ControllingPlayer == player)
            {
                var baseController = vehicle.Vehicle_BaseController;
                var combatController = vehicle.Vehicle_CombatController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;

                if (haveBaseController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(baseController, entity.EntityId, player.PlayerId);
                }

                if (haveCombatController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(combatController, entity.EntityId, player.PlayerId);
                }
            }

            var observer = vehicle.Vehicle_ObserverView;
            var combat = vehicle.Vehicle_CombatView;
            var movement = vehicle.Vehicle_MovementView;

            bool haveObserver = observer != null;
            bool haveCombat = combat != null;
            bool haveMovement = movement != null;

            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }

            if (haveCombat)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(combat, entity.EntityId);
            }

            if (haveMovement)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(movement, entity.EntityId);
            }
        }
        else if (entity is DeployableEntity deployable)
        {
            var observer = deployable.Deployable_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.UnreliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is TurretEntity turret)
        {
            if (turret.IsPlayerControlled && turret.ControllingPlayer == player)
            {
                var baseController = turret.Turret_BaseController;

                bool haveBaseController = baseController != null;

                if (haveBaseController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(baseController, entity.EntityId, player.PlayerId);
                }
            }

            var observer = turret.Turret_ObserverView;

            bool haveObserver = observer != null;

            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is OutpostEntity outpost)
        {
            var observer = outpost.Outpost_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is ThumperEntity thumper)
        {
            var observer = thumper.ResourceNode_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is CarryableEntity carryable)
        {
            var observer = carryable.CarryableObject_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }
        }
        else if (entity is AreaVisualDataEntity avd)
        {
            var observer = avd.AreaVisualData_ObserverView;
            if (observer != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(observer, entity.EntityId);
            }

            var particleEffects = avd.AreaVisualData_ParticleEffectsView;
            if (particleEffects != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(particleEffects, entity.EntityId);
            }

            var mapMarker = avd.AreaVisualData_MapMarkerView;
            if (mapMarker != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(mapMarker, entity.EntityId);
            }

            var tinyObject = avd.AreaVisualData_TinyObjectView;
            if (tinyObject != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(tinyObject, entity.EntityId);
            }

            var lootObject = avd.AreaVisualData_LootObjectView;
            if (lootObject != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(lootObject, entity.EntityId);
            }

            var forceShield = avd.AreaVisualData_ForceShieldView;
            if (forceShield != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendViewScopeOut(forceShield, entity.EntityId);
            }
        }
    }

    public void RemoveControllers(INetworkPlayer player, IEntity entity)
    {
        if (entity is VehicleEntity vehicle)
        {
            if (vehicle.IsPlayerControlled && vehicle.ControllingPlayer == player)
            {
                var baseController = vehicle.Vehicle_BaseController;
                var combatController = vehicle.Vehicle_CombatController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;

                if (haveBaseController && haveCombatController)
                {
                    // By forcing a flush we ensure that the update to the vehicle state to remove the controlling player are sent down before we remove the controllers. Without this the order of messages won't match the capture and it doesn't behave as we want.
                    FlushViewChangesToPlayer(baseController, entity.EntityId, player);
                    FlushViewChangesToPlayer(combatController, entity.EntityId, player);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(combatController, entity.EntityId, player.PlayerId);
                }
            }
        }
        else if (entity is TurretEntity turret)
        {
            if (turret.IsPlayerControlled && turret.ControllingPlayer == player)
            {
                var baseController = turret.Turret_BaseController;

                bool haveBaseController = baseController != null;

                if (haveBaseController)
                {
                    // By forcing a flush we ensure that the update to the turret state to remove the controlling player are sent down before we remove the controllers. Without this the order of messages won't match the capture and it doesn't behave as we want.
                    FlushViewChangesToPlayer(baseController, entity.EntityId, player);
                    player.NetChannels[ChannelType.ReliableGss].SendControllerRemove(baseController, entity.EntityId, player.PlayerId);
                }
            }
        }
    }

    public void FlushChanges(IEntity entity)
    {
        if (entity is CharacterEntity character)
        {
            if (character.IsPlayerControlled)
            {
                FlushViewChangesToPlayer(character.Character_BaseController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_CombatController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_MissionAndMarkerController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_LocalEffectsController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_SpectatorController, character.EntityId, character.Player);
            }

            // We don't flush Character_MovementView as those changes are basically handled entirely by CurrentPoseUpdate
            FlushViewChangesToScoped(character.Character_ObserverView, character.EntityId);
            FlushViewChangesToScoped(character.Character_EquipmentView, character.EntityId);
            FlushViewChangesToScoped(character.Character_CombatView, character.EntityId);
            FlushViewChangesToScoped(character.Character_TinyObjectView, character.EntityId);
        }
        else if (entity is MeldingEntity melding)
        {
            FlushViewChangesToScoped(melding.Melding_ObserverView, melding.EntityId);
        }
        else if (entity is MeldingBubbleEntity meldingBubble)
        {
            FlushViewChangesToScoped(meldingBubble.MeldingBubble_ObserverView, meldingBubble.EntityId);
        }
        else if (entity is VehicleEntity vehicle)
        {
            if (vehicle.IsPlayerControlled)
            {
                FlushViewChangesToPlayer(vehicle.Vehicle_BaseController, vehicle.EntityId, vehicle.ControllingPlayer);
                FlushViewChangesToPlayer(vehicle.Vehicle_CombatController, vehicle.EntityId, vehicle.ControllingPlayer);
            }

            FlushViewChangesToScoped(vehicle.Vehicle_ObserverView, vehicle.EntityId);
            FlushViewChangesToScoped(vehicle.Vehicle_CombatView, vehicle.EntityId);
            FlushViewChangesToScoped(vehicle.Vehicle_MovementView, vehicle.EntityId);
        }
        else if (entity is DeployableEntity deployable)
        {
            FlushViewChangesToScoped(deployable.Deployable_ObserverView, deployable.EntityId);
        }
        else if (entity is TurretEntity turret)
        {
            if (turret.IsPlayerControlled)
            {
                FlushViewChangesToPlayer(turret.Turret_BaseController, turret.EntityId, turret.ControllingPlayer);
            }

            FlushViewChangesToScoped(turret.Turret_ObserverView, turret.EntityId);
        }
        else if (entity is ThumperEntity thumper)
        {
            FlushViewChangesToScoped(thumper.ResourceNode_ObserverView, thumper.EntityId);
        }
        else if (entity is AreaVisualDataEntity avd)
        {
            FlushViewChangesToScoped(avd.AreaVisualData_ObserverView, avd.EntityId);
            FlushViewChangesToScoped(avd.AreaVisualData_ParticleEffectsView, avd.EntityId);
            FlushViewChangesToScoped(avd.AreaVisualData_MapMarkerView, avd.EntityId);
            FlushViewChangesToScoped(avd.AreaVisualData_TinyObjectView, avd.EntityId);
            FlushViewChangesToScoped(avd.AreaVisualData_LootObjectView, avd.EntityId);
            FlushViewChangesToScoped(avd.AreaVisualData_ForceShieldView, avd.EntityId);
        }
    }

    public void FlushViewChangesToPlayer<TPacket>(TPacket view, ulong entityId, INetworkPlayer player)
    where TPacket : class, IAeroViewInterface
    {
        // Just to reduce repetition
        bool shouldFlush = view != null && view.GetPackedChangesSize() > 0;
        bool shouldSend = player.Status.Equals(IPlayer.PlayerStatus.Playing) || player.Status.Equals(IPlayer.PlayerStatus.Loading);
        if (shouldFlush && shouldSend)
        {
            player.NetChannels[ChannelType.ReliableGss].SendChanges(view, entityId);
        }
    }

    public void FlushViewChangesToScoped<TPacket>(TPacket view, ulong entityId)
    where TPacket : class, IAeroViewInterface
    {
        // We can only call SerializeChangesToMemory once but we need to send to multiple players.
        bool shouldFlush = view != null && view.GetPackedChangesSize() > 0;
        if (shouldFlush)
        {
            view.SerializeChangesToMemory(out var update);
            foreach (var client in ScopedPlayersByEntity[entityId])
            {
                bool shouldSend = client.Status.Equals(IPlayer.PlayerStatus.Playing) || client.Status.Equals(IPlayer.PlayerStatus.Loading);
                if (shouldSend)
                {
                    client.NetChannels[ChannelType.UnreliableGss].SendChanges(view, entityId, update);
                }
            }
        }
    }
 
    private void OnAddedEntity(IEntity entity)
    {
        // TEMP: Hack to introduce new entities to connected players. This should be replaced with tick logic that sends down entities based on scope and distance.
        foreach (var client in Shard.Clients.Values)
        {
            // We don't want to inform players that are still in the early steps of connecting
            if (client.CanReceiveGSS)
            {
                ScopeIn(client, entity);
            }
        }
    }

    private void OnRemovedEntity(IEntity entity)
    {
        foreach (var client in ScopedPlayersByEntity[entity.EntityId])
        {
            ScopeOut(client, entity);
        }
    }

    private class ScopeInRequest
    {
        public INetworkPlayer Player;
        public IEntity Entity;
    }

    private class Lifetime
    {
        public ulong ExpireAt;
    }
}