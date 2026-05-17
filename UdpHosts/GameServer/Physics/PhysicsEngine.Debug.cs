#nullable enable
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using DebugPipeProto;
using Serilog;

namespace GameServer.Physics;

/// <summary>
///    Debug
/// </summary>
public partial class PhysicsEngine
{
    private readonly ConcurrentQueue<PipeMessage> _debugMessageQueue = new();
    private DebugPipeServer? DebugPipe { get; set; }

    public void DebugOnMessage(PipeMessage msg)
    {
        _debugMessageQueue.Enqueue(msg);
    }

    public void DebugSendLoadEntities()
    {
        if (DebugPipe == null)
        {
            return;
        }

        List<CreateKineticEntity> entities = [];
        foreach(var (entityId, bodyHandle) in _entityIdToBody)
        {
            ref var currentPose = ref Simulation.Bodies[bodyHandle].Pose;
            var key = _entityIdToAssetKey[entityId];
            var entity = new CreateKineticEntity
            {
                EntityId = entityId,
                Pose = currentPose.ToProto(),
                Shape = new PipeCollisionShape
                {
                    AssetId = key.AssetId,
                    Offset = key.Offset.ToProto(),
                    Scale = key.Scale,
                },
            };
            entities.Add(entity);
        }

        _ = DebugPipe?.SendAsync(new PipeMessage
        {
            LoadEntities = new LoadEntities
            {
                Entities = { entities }
            }
        });
    }

    public void DebugSendTickUpdate()
    {
        if (DebugPipe == null)
        {
            return;
        }

        List<UpdateEntity> updates = [];
        foreach(var (entityId, bodyHandle) in _entityIdToBody)
        {
            ref var currentPose = ref Simulation.Bodies[bodyHandle].Pose;
            var key = _entityIdToAssetKey[entityId];
            var update = new UpdateEntity
            {
                EntityId = entityId,
                Pose = currentPose.ToProto(),
                Shape = new PipeCollisionShape
                {
                    AssetId = key.AssetId,
                    Offset = key.Offset.ToProto(),
                    Scale = key.Scale,
                },
            };
            updates.Add(update);
        }

        _ = DebugPipe?.SendAsync(new PipeMessage
        {
            TickUpdate = new TickUpdate
            {
                Updates = { updates }
            }
        });
    }

    public BodyHandle DebugViewCreateKineticEntity(ulong entityId, RigidPose pose, PipeCollisionShape pipeShape)
    {
        _logger.Debug("DebugViewCreateKineticEntity {entityId} {pipeShape}", entityId, pipeShape);
        var key = new AssetCompoundKey(pipeShape.AssetId, pipeShape.Offset.ToNumerics(), pipeShape.Scale);
        var shape = GetAssetShape(key);
        var body = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, shape, -1));
        _bodyToEntityId[body] = entityId;
        _entityIdToBody[entityId] = body;
        _entityIdToAssetKey[entityId] = key;
        return body;
    }

    public void DebugViewUpdateEntity(ulong entityId, RigidPose pose, PipeCollisionShape pipeShape)
    {
        if (!_entityIdToBody.ContainsKey(entityId))
        {
            return;
        }

        var bodyHandle = _entityIdToBody[entityId];
        var body = Simulation.Bodies[bodyHandle];
        ref var currentPose = ref body.Pose;
        var currentShape = body.Collidable.Shape;

        var currentKey = _entityIdToAssetKey[entityId];
        var pipeKey = new AssetCompoundKey(pipeShape.AssetId, pipeShape.Offset.ToNumerics(), pipeShape.Scale);

        if (currentPose.Position != pose.Position || currentPose.Orientation != pose.Orientation || !currentKey.Equals(pipeKey))
        {
            var shape = GetAssetShape(pipeKey);
            body.Awake = true;
            body.SetShape(shape);
            currentPose.Position = pose.Position;
            currentPose.Orientation = pose.Orientation;
        }
    }

    public void DebugViewRemoveEntity(ulong entityId)
    {
        if (!_entityIdToBody.ContainsKey(entityId))
        {
            _logger.Warning("RemoveEntity was called for {entityId} but there is no body!", entityId);
            return;
        }

        var bodyHandle = _entityIdToBody[entityId];
        _entityIdToAssetKey.Remove(entityId);
        _entityIdToBody.Remove(entityId);
        _bodyToEntityId.Remove(bodyHandle);
        Simulation.Bodies.Remove(bodyHandle);
    }

    public bool DebugEntityTryGetNext(out BodyHandle value)
    {
        List<ulong> keys = [.. _entityIdToBody.Keys];

        if (keys.Count == 0)
        {
            value = default;
            return false;
        }

        _debugEntityIndex++;

        if (_debugEntityIndex >= keys.Count)
        {
            _debugEntityIndex = 0;
        }

        ulong key = keys[_debugEntityIndex];
        value = _entityIdToBody[key];
        return true;
    }

    public bool DebugEntityTryGetPrevious(out BodyHandle value)
    {
        List<ulong> keys = [.. _entityIdToBody.Keys];

        if (keys.Count == 0)
        {
            value = default;
            return false;
        }

        _debugEntityIndex--;

        if (_debugEntityIndex < 0)
        {
            _debugEntityIndex = keys.Count - 1;
        }

        ulong key = keys[_debugEntityIndex];
        value = _entityIdToBody[key];
        return true;
    }

    partial void DebugInitialize(bool isDebugPipeClient, uint zoneId)
    {
        if (!isDebugPipeClient)
        {
            _logger.Debug("PhysicsEngine starting DebugPipeServer");
            DebugPipe = new DebugPipeServer(Log.Logger.ForContext<DebugPipeServer>());
            DebugPipe.OnConnection += () =>
            {
                _ = DebugPipe?.SendAsync(new PipeMessage
                {
                    LoadZone = new LoadZone
                    {
                        ZoneId = zoneId
                    }
                });
                DebugSendLoadEntities();
            };
            DebugPipe.OnMessage += (msg) =>
            {
                /** These handlers are for message sent from the debug client to the physics server */
                switch (msg.PayloadCase)
                {
                    case PipeMessage.PayloadOneofCase.ZoneReady:
                        DebugSendLoadEntities();
                        break;

                    default:
                        _logger.Debug("Unhandled message {msg}", msg);
                        break;
                }
            };
            var cts = new CancellationTokenSource();
            _ = Task.Run(() => DebugPipe.RunAsync(cts.Token));
        }
    }

    private void DebugProcessMessages()
    {
        while (_debugMessageQueue.TryDequeue(out var msg))
        {
            /** These handlers are for message sent from the physics server to the debug client */
            switch (msg.PayloadCase)
            {
                case PipeMessage.PayloadOneofCase.LoadZone:
                    LoadZone(msg.LoadZone.ZoneId, _mapsPath);
                    break;

                case PipeMessage.PayloadOneofCase.LoadEntities:
                    foreach (var entity in msg.LoadEntities.Entities)
                    {
                        DebugViewCreateKineticEntity(entity.EntityId, entity.Pose.ToBepu(), entity.Shape);
                    }

                    break;

                case PipeMessage.PayloadOneofCase.TickUpdate:
                    foreach (var update in msg.TickUpdate.Updates)
                    {
                        DebugViewUpdateEntity(update.EntityId, update.Pose.ToBepu(), update.Shape);
                    }

                    break;

                case PipeMessage.PayloadOneofCase.CreateKineticEntity:
                    DebugViewCreateKineticEntity(msg.CreateKineticEntity.EntityId, msg.CreateKineticEntity.Pose.ToBepu(), msg.CreateKineticEntity.Shape);
                    break;

                case PipeMessage.PayloadOneofCase.UpdateEntity:
                    DebugViewUpdateEntity(msg.UpdateEntity.EntityId, msg.UpdateEntity.Pose.ToBepu(), msg.UpdateEntity.Shape);
                    break;

                case PipeMessage.PayloadOneofCase.RemoveEntity:
                    DebugViewRemoveEntity(msg.RemoveEntity.EntityId);
                    break;

                default:
                    _logger.Debug("Unhandled message {msg}", msg);
                    break;
            }
        }
    }
}