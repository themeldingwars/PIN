using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Aero.Gen;
using AeroMessages.Common;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Melding.View;
using GameServer.Aptitude;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities;
using GameServer.Entities.Deployable;
using GameServer.Entities.Melding;
using GameServer.Entities.Outpost;
using GameServer.Entities.Vehicle;
using GameServer.Extensions;

namespace GameServer;

public class EntityManager
{
    private const byte ServerId = 31;
    private Shard Shard;
    private uint Counter = 0;

    private ulong LastUpdateFlush = 0;
    private ulong UpdateFlushIntervalMs = 5;
    private bool hasSpawnedTestEntities = false;

    public EntityManager(Shard shard)
    {
        Shard = shard;
    }

    public void SpawnVehicle(ushort typeId, Vector3 position, Quaternion orientation, IEntity owner, bool autoMount = false)
    {
        var vehicleInfo = SDBUtils.GetDetailedVehicleInfo(typeId);
        var vehicleEntity = new VehicleEntity(Shard, GetNextGuid() & 0xffffffffffffff00);
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
        if (owner != null)
        {
            vehicleEntity.SetOwner(owner as BaseEntity);
            
            if (owner.GetType() == typeof(Entities.Character.CharacterEntity))
            {
                var character = owner as Entities.Character.CharacterEntity;
                if (character.IsPlayerControlled)
                {
                    vehicleEntity.SetOwningPlayer(character.Player);
                }
            }

            if (autoMount)
            {
                // TODO:
            }
        }

        Add(vehicleEntity.EntityId, vehicleEntity);

        if (vehicleEntity.SpawnAbility != 0)
        {
            Shard.Abilities.HandleActivateAbility(Shard, vehicleEntity, vehicleEntity.SpawnAbility, Shard.CurrentTime, new HashSet<IAptitudeTarget>());
        }
    }

    public void SpawnDeployable(uint typeId, Vector3 position, Quaternion orientation)
    {
        var deployableInfo = SDBInterface.GetDeployable(typeId);
        var deployableEntity = new DeployableEntity(Shard,  GetNextGuid() & 0xffffffffffffff00, typeId, 0);
        var aimDirection = new Vector3(deployableInfo.AimDirection.x, deployableInfo.AimDirection.y, deployableInfo.AimDirection.z);
        deployableEntity.SetPosition(position);
        deployableEntity.SetOrientation(orientation);
        deployableEntity.SetAimDirection(aimDirection);
        Add(deployableEntity.EntityId, deployableEntity);

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

        if (deployableInfo.SpawnAbilityid != 0)
        {
            Shard.Abilities.HandleActivateAbility(Shard, deployableEntity, deployableInfo.SpawnAbilityid, Shard.CurrentTime, new HashSet<IAptitudeTarget>());
        }

        if (deployableInfo.ConstructedAbilityid != 0)
        {
            new Timer(state =>
            {
                Shard.Abilities.HandleActivateAbility(Shard, deployableEntity, deployableInfo.ConstructedAbilityid, Shard.CurrentTime, new HashSet<IAptitudeTarget>());
                ((Timer)state).Dispose();
            },
            null,
            deployableInfo.BuildTimeMs,
            Timeout.Infinite);
        }
    }

    public void SpawnMelding(string perimiterSetName, ActiveDataStruct activeData)
    {
        var meldingEntity = new MeldingEntity(Shard,  GetNextGuid() & 0xffffffffffffff00, perimiterSetName);
        meldingEntity.SetActiveData(activeData);
        Add(meldingEntity.EntityId, meldingEntity);
    }

    public void SpawnOutpost(Outpost outpost)
    {
        var outpostEntity = new OutpostEntity(Shard,  GetNextGuid() & 0xffffffffffffff00, outpost);
        Add(outpostEntity.EntityId, outpostEntity);
    }

    public void TempSpawnTestEntities()
    {
        // NewYou
        SpawnDeployable(676, new Vector3(177.37387f, 239.90964f, 491.86758f), new Quaternion(-4.0596834E-08f, -1.620471E-08f, -0.37071967f, 0.92874485f));

        // Battleframe Station
        SpawnDeployable(395, new Vector3(170.84642f, 243.20822f, 491.71597f), new Quaternion(0f, 0f, 0.92874485f, 0.37071964f));

        // Garage
        SpawnDeployable(44, new Vector3(185.69464f, 247.1212f, 491.86783f), new Quaternion(1.6204714E-08f, -4.059684E-08f, 0.92874485f, 0.37071967f));

        // Glider Pad
        SpawnDeployable(372, new Vector3(164.84642f, 262.20822f, 491.71597f), new Quaternion(0f, 0f, 0.92874485f, 0.37071964f));

        // Melding
        foreach (var entry in CustomDBInterface.GetZoneMeldings(448))
        {
            var melding = entry.Value;
            SpawnMelding(melding.PerimiterSetName, new ActiveDataStruct()
            {
                Unk1 = melding.Unk1,
                Unk2 = melding.Unk2,
                Unk3 = melding.Unk3,
                ControlPoints_1 = melding.ControlPoints,
                Offsets_1 = melding.Offsets,
                ControlPoints_2 = melding.ControlPoints,
                Offsets_2 = melding.Offsets,
            });
        }

        // Outpost
        foreach (var entry in CustomDBInterface.GetZoneOutposts(448))
        {
            var outpost = entry.Value;
            SpawnOutpost(outpost);
        }
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Spawn test entities on first real tick
        if (!hasSpawnedTestEntities && currentTime != 0)
        {
            hasSpawnedTestEntities = true;
            TempSpawnTestEntities();
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
    }

    public ulong GetNextGuid()
    {
        return new Core.Data.EntityGuid(ServerId, Shard.CurrentTime, Counter++, (byte)Enums.GSS.Controllers.Character).Full;
    }

    public void Add(ulong guid, Entities.IEntity entity)
    {
        Shard.Entities.Add(guid, entity);
        OnAddedEntity(entity);
    }

    public void Add(Entities.IEntity entity)
    {
        var guid = new Core.Data.EntityGuid(ServerId, Shard.CurrentTime, Counter++, (byte)Enums.GSS.Controllers.Character);
        Shard.Entities.Add(guid.Full, entity);
        OnAddedEntity(entity);
    }

    public void Remove(Entities.IEntity entity)
    {
        Remove(entity.EntityId);
    }

    public void Remove(Core.Data.EntityGuid guid)
    {
        Remove(guid.Full);
    }

    public void Remove(ulong guid)
    {
        Shard.Entities.Remove(guid);
    }

    public void ScopeInAll(INetworkPlayer player)
    {
        foreach (var entity in Shard.Entities.Values)
        {
            if (entity == player.CharacterEntity)
            {
                continue; // Hack: Prevent ScopeInAll from triggering keyframes for the local player's character. We have already sent down those in the login process, repeating is wasteful but also causes some issues if we don't properly persist the changes made to the views during respawn.
            }

            ScopeIn(player, entity);
        }
    }

    public void ScopeIn(INetworkPlayer player, IEntity entity)
    {
        if (entity.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = entity as Entities.Character.CharacterEntity;

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
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(combatController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(effectsController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(missionController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAero(new CharacterLoaded(), entity.EntityId);
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
                player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
                player.NetChannels[ChannelType.ReliableGss].SendIAero(equipment, entity.EntityId, 3);
                player.NetChannels[ChannelType.ReliableGss].SendIAero(combat, entity.EntityId, 3);
                player.NetChannels[ChannelType.ReliableGss].SendIAero(movement, entity.EntityId, 3);
            }

            if (haveTinyObject)
            {
                player.NetChannels[ChannelType.ReliableGss].SendIAero(tinyobject, entity.EntityId, 3);
            }
        }
        else if (entity.GetType() == typeof(Entities.Melding.MeldingEntity))
        {
            var melding = entity as Entities.Melding.MeldingEntity;
            var observer = melding.Melding_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
            }
        }
        else if (entity.GetType() == typeof(Entities.MeldingBubble.MeldingBubbleEntity))
        {
            var meldingBubble = entity as Entities.MeldingBubble.MeldingBubbleEntity;
            var observer = meldingBubble.MeldingBubble_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
            }
        }
        else if (entity.GetType() == typeof(Entities.Vehicle.VehicleEntity))
        {
            var vehicle = entity as Entities.Vehicle.VehicleEntity;

            if (vehicle.IsPlayerControlled && vehicle.ControllingPlayer == player)
            {
                var baseController = vehicle.Vehicle_BaseController;
                var combatController = vehicle.Vehicle_CombatController;

                bool haveBaseController = baseController != null;
                bool haveCombatController = combatController != null;

                if (haveBaseController && haveCombatController)
                {
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(combatController, entity.EntityId, player.PlayerId);
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
                player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
                player.NetChannels[ChannelType.ReliableGss].SendIAero(combat, entity.EntityId, 3);
                player.NetChannels[ChannelType.ReliableGss].SendIAero(movement, entity.EntityId, 3);
            }
        }
        else if (entity.GetType() == typeof(Entities.Deployable.DeployableEntity))
        {
            var deployable = entity as Entities.Deployable.DeployableEntity;
            var observer = deployable.Deployable_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
            }
        }
        else if (entity.GetType() == typeof(Entities.Outpost.OutpostEntity))
        {
            var outpost = entity as Entities.Outpost.OutpostEntity;
            var observer = outpost.Outpost_ObserverView;
            bool haveObserver = observer != null;
            if (haveObserver)
            {
                 player.NetChannels[ChannelType.ReliableGss].SendIAero(observer, entity.EntityId, 3);
            }
        }
    }

    public void RemoveControllers(INetworkPlayer player, IEntity entity)
    {
        if (entity.GetType() == typeof(Entities.Vehicle.VehicleEntity))
        {
            var vehicle = entity as Entities.Vehicle.VehicleEntity;

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
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerRemove(baseController, entity.EntityId, player.PlayerId);
                    player.NetChannels[ChannelType.ReliableGss].SendIAeroControllerRemove(combatController, entity.EntityId, player.PlayerId);
                }
            }
        }
    }

    public void FlushChanges(IEntity entity)
    {
        if (entity.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = entity as Entities.Character.CharacterEntity;

            if (character.IsPlayerControlled)
            {
                FlushViewChangesToPlayer(character.Character_BaseController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_CombatController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_MissionAndMarkerController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_LocalEffectsController, character.EntityId, character.Player);
                FlushViewChangesToPlayer(character.Character_SpectatorController, character.EntityId, character.Player);
            }

            // We don't flush Character_MovementView as those changes are basically handled entirely by CurrentPoseUpdate
            FlushViewChangesToEveryone(character.Character_ObserverView, character.EntityId);
            FlushViewChangesToEveryone(character.Character_EquipmentView, character.EntityId);
            FlushViewChangesToEveryone(character.Character_CombatView, character.EntityId);
            FlushViewChangesToEveryone(character.Character_TinyObjectView, character.EntityId);
        }
        else if (entity.GetType() == typeof(Entities.Melding.MeldingEntity))
        {
            var melding = entity as Entities.Melding.MeldingEntity;

            FlushViewChangesToEveryone(melding.Melding_ObserverView, melding.EntityId);
        }
        else if (entity.GetType() == typeof(Entities.MeldingBubble.MeldingBubbleEntity))
        {
            var meldingBubble = entity as Entities.MeldingBubble.MeldingBubbleEntity;

            FlushViewChangesToEveryone(meldingBubble.MeldingBubble_ObserverView, meldingBubble.EntityId);
        }
        else if (entity.GetType() == typeof(Entities.Vehicle.VehicleEntity))
        {
            var vehicle = entity as Entities.Vehicle.VehicleEntity;

            if (vehicle.IsPlayerControlled)
            {
                FlushViewChangesToPlayer(vehicle.Vehicle_BaseController, vehicle.EntityId, vehicle.ControllingPlayer);
                FlushViewChangesToPlayer(vehicle.Vehicle_CombatController, vehicle.EntityId, vehicle.ControllingPlayer);
            }

            FlushViewChangesToEveryone(vehicle.Vehicle_ObserverView, vehicle.EntityId);
            FlushViewChangesToEveryone(vehicle.Vehicle_CombatView, vehicle.EntityId);
            FlushViewChangesToEveryone(vehicle.Vehicle_MovementView, vehicle.EntityId);
        }
        else if (entity.GetType() == typeof(Entities.Deployable.DeployableEntity))
        {
            var deployable = entity as Entities.Deployable.DeployableEntity;
            FlushViewChangesToEveryone(deployable.Deployable_ObserverView, deployable.EntityId);
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
            player.NetChannels[ChannelType.ReliableGss].SendIAeroChanges(view, entityId);
        }
    }

    public void FlushViewChangesToEveryone<TPacket>(TPacket view, ulong entityId)
    where TPacket : class, IAeroViewInterface
    {
        // We can only call SerializeChangesToMemory once but we need to send to multiple players.
        bool shouldFlush = view != null && view.GetPackedChangesSize() > 0;
        if (shouldFlush)
        {
            view.SerializeChangesToMemory(out var update);
            foreach (var client in Shard.Clients.Values)
            {
                bool shouldSend = client.Status.Equals(IPlayer.PlayerStatus.Playing) || client.Status.Equals(IPlayer.PlayerStatus.Loading);
                if (shouldSend)
                {
                    client.NetChannels[ChannelType.UnreliableGss].SendIAeroChanges(view, entityId, update);
                }
            }
        }
    }
 
    private void OnAddedEntity(Entities.IEntity entity)
    {
        // TEMP: Hack to introduce new entities to connected players. This should be replaced with tick logic that sends down entities based on scope and distance.
        foreach (var client in Shard.Clients.Values)
        {
            // We don't want to inform players that are still in the early steps of connecting
            if (client.Status.Equals(IPlayer.PlayerStatus.Playing) || client.Status.Equals(IPlayer.PlayerStatus.Loading))
            {
                ScopeIn(client, entity);
            }
        }
    }
}