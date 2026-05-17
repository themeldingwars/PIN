#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using DebugPipeProto;
using GameServer.Data.SDB;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Physics;

/// <summary>
///    Main
/// </summary>
public partial class PhysicsEngine
{
    public const float TargetTimestepDuration = 50; // (1/20f)

    private readonly ILogger _logger;
    private readonly EventBus _eventBus;
    private readonly Dictionary<BodyHandle, ulong> _bodyToEntityId = [];
    private readonly Dictionary<ulong, BodyHandle> _entityIdToBody = [];
    private readonly Dictionary<ulong, AssetCompoundKey> _entityIdToAssetKey = [];
    private readonly string _assetsPath = string.Empty;
    private readonly string _mapsPath = string.Empty;
    private TypedIndex _fallbackShape;
    private int _debugEntityIndex = -1;

    public PhysicsEngine(EventBus eventBus, uint zoneId, string mapsPath = "", string assetDBPath = "", string assetsPath = "", bool loadMapsCollision = false, DebugProjectileHitCallbacks? debugProjectileHitCallbacks = null, bool isDebugPipeClient = false)
    {
        _eventBus = eventBus;
        _logger = Log.Logger.ForContext<PhysicsEngine>();
        _assetsPath = assetsPath;
        _mapsPath = mapsPath;
        DebugProjectileHitCallbacks = debugProjectileHitCallbacks;

        // Determine number of threads to use
        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        // Setup BepuPhysics
        BufferPool = new BufferPool();
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        // Default shapes
        _fallbackShape = Simulation.Shapes.Add(new Sphere(0.9f));

        // Construct loaders
        TagfileLoader = new TagfileLoader.TagfileLoader(Simulation, BufferPool, ThreadDispatcher);
        ZoneLoader = new ZoneLoader.ZoneLoader(Simulation, BufferPool, ThreadDispatcher, TagfileLoader);
        PoseLoader = new PoseLoader.PoseLoader(assetDBPath);

        DebugInitialize(isDebugPipeClient, zoneId);

        // Load zone
        if (loadMapsCollision)
        {
            LoadZone(zoneId, mapsPath);
        }
    }

    public event Action? OnZoneLoaded;

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }
    public ThreadDispatcher ThreadDispatcher { get; private set; }
    public double TimeAccumulator { get; protected set; }
    public TagfileLoader.TagfileLoader TagfileLoader { get; private set; }
    public ZoneLoader.ZoneLoader ZoneLoader { get; private set; }
    public PoseLoader.PoseLoader PoseLoader { get; private set; }
    private DebugProjectileHitCallbacks? DebugProjectileHitCallbacks { get; set; }

    public void LoadZone(uint zoneId, string mapsPath = "")
    {
        _logger.Debug("LoadZone {zoneId}", zoneId);
        var cachePath = Physics.ZoneLoader.SimulationCache.GetCachePath(mapsPath, zoneId);
        if (!Physics.ZoneLoader.SimulationCache.TryLoad(Simulation, BufferPool, ThreadDispatcher, cachePath))
        {
            var ok = ZoneLoader.LoadCollision(mapsPath, zoneId);
            if (ok)
            {
                Physics.ZoneLoader.SimulationCache.Save(Simulation, BufferPool, cachePath);
            }
        }

        OnZoneLoaded?.Invoke();
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        TimeAccumulator += deltaTime;
        while (!ct.IsCancellationRequested && TimeAccumulator >= TargetTimestepDuration)
        {
            DebugProcessMessages();
            Simulation.Timestep(TargetTimestepDuration, ThreadDispatcher);
            TimeAccumulator -= TargetTimestepDuration;
            DebugSendTickUpdate();
        }
    }

    public BodyHandle CreateKineticEntity(CharacterEntity entity)
    {
        _logger.Debug("CreateKineticEntity Character {entityId}", entity.EntityId);
        var pose = new RigidPose { Position = entity.Position, Orientation = Quaternion.Inverse(entity.Orientation) };
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);
        var body = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, shape, -1));
        _bodyToEntityId[body] = entity.EntityId;
        _entityIdToBody[entity.EntityId] = body;
        _entityIdToAssetKey[entity.EntityId] = key;

        _ = DebugPipe?.SendAsync(new PipeMessage
        {
            CreateKineticEntity = new CreateKineticEntity
            {
                EntityId = entity.EntityId,
                Pose = pose.ToProto(),
                Shape = new PipeCollisionShape
                {
                    AssetId = key.AssetId,
                    Offset = key.Offset.ToProto(),
                    Scale = key.Scale,
                },
            }
        });

        return body;
    }

    public BodyHandle CreateKineticEntity(BaseEntity entity)
    {
        _logger.Debug("CreateKineticEntity Base {entityId}", entity.EntityId);
        var assetId = entity.Collision.HitboxCollisionId;
        var offset = Vector3.Zero;
        var scale = entity.Collision.Scale;
        var pose = new RigidPose { Position = entity.Position, Orientation = Quaternion.Inverse(entity.Orientation) };
        var key = new AssetCompoundKey(assetId, offset, scale);
        var shape = GetAssetShape(key);
        var body = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, shape, -1));
        _bodyToEntityId[body] = entity.EntityId;
        _entityIdToBody[entity.EntityId] = body;
        _entityIdToAssetKey[entity.EntityId] = key;

        _ = DebugPipe?.SendAsync(new PipeMessage
        {
            CreateKineticEntity = new CreateKineticEntity
            {
                EntityId = entity.EntityId,
                Pose = pose.ToProto(),
                Shape = new PipeCollisionShape
                {
                    AssetId = assetId,
                    Offset = offset.ToProto(),
                    Scale = scale,
                }
            }
        });

        return body;
    }

    public void UpdateEntity(CharacterEntity entity)
    {
        if (!_entityIdToBody.ContainsKey(entity.EntityId))
        {
            return;
        }

        var bodyHandle = _entityIdToBody[entity.EntityId];
        var body = Simulation.Bodies[bodyHandle];
        ref var currentPose = ref body.Pose;
        var currentShape = body.Collidable.Shape;
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);

        var orientation = Quaternion.Inverse(entity.Orientation);
        if (currentPose.Position != entity.Position || currentPose.Orientation != orientation || currentShape != shape)
        {
            _entityIdToAssetKey[entity.EntityId] = key;
            body.Awake = true;
            body.SetShape(shape);
            currentPose.Position = entity.Position;
            currentPose.Orientation = orientation;
        }
    }

    public void UpdateEntity(BaseEntity entity)
    {
        if (!_entityIdToBody.ContainsKey(entity.EntityId))
        {
            return;
        }

        var bodyHandle = _entityIdToBody[entity.EntityId];
        ref var currentPose = ref Simulation.Bodies[bodyHandle].Pose;

        var orientation = Quaternion.Inverse(entity.Orientation);
        if (currentPose.Position != entity.Position || currentPose.Orientation != orientation)
        {
            var body = Simulation.Bodies[bodyHandle];
            body.Awake = true;
            currentPose.Position = entity.Position;
            currentPose.Orientation = orientation;
        }
    }

    public void RemoveEntity(IEntity entity)
    {
        if (!_entityIdToBody.ContainsKey(entity.EntityId))
        {
            _logger.Warning("RemoveEntity was called for {entity} but there is no body!", entity.ToString());
            return;
        }

        var bodyHandle = _entityIdToBody[entity.EntityId];
        _entityIdToAssetKey.Remove(entity.EntityId);
        _entityIdToBody.Remove(entity.EntityId);
        _bodyToEntityId.Remove(bodyHandle);
        Simulation.Bodies.Remove(bodyHandle);

        _ = DebugPipe?.SendAsync(new PipeMessage
        {
            RemoveEntity = new RemoveEntity
            {
                EntityId = entity.EntityId,
            }
        });
    }

    public void ProjectileRayCast(Vector3 origin, Vector3 direction, CharacterEntity source, uint trace)
    {
        var speed = 500f;
        var maxRange = 500f;

        DebugProjectileHitCallbacks?.SendDebugProjectileSpawn(source, trace, origin, direction, speed);

        var hitHandler = default(RayHitHandler);
        hitHandler.T = maxRange;
        hitHandler.AvoidSourceBody = true;
        hitHandler.SourceBody = _entityIdToBody[source.EntityId];

        Simulation.RayCast(origin, direction, float.MaxValue, BufferPool, ref hitHandler);
        if (hitHandler.T < maxRange)
        {
            var hitPosition = origin + (direction * hitHandler.T);
            _logger.Debug("HitHandler {Mobility} T {T} HitCollidable {HitCollidable} at {HitPosition}", hitHandler.HitCollidable.Mobility, hitHandler.T, hitHandler.HitCollidable, hitPosition);

            DebugProjectileHitCallbacks?.SendDebugProjectileImpact(source, trace, hitPosition, hitHandler.Normal);

            if (hitHandler.HitCollidable.Mobility == CollidableMobility.Kinematic)
            {
                var bodyPosition = Simulation.Bodies[hitHandler.HitCollidable.BodyHandle].Pose.Position;
                bodyPosition.Z -= 0.9f;
                DebugProjectileHitCallbacks?.SendDebugProjectilePoseHit(source, trace, hitPosition, bodyPosition);

                // Temporary debug testing below
                var hitEntityId = _bodyToEntityId.GetValueOrDefault(hitHandler.HitCollidable.BodyHandle);
                if (hitEntityId != 0)
                {
                    var body = Simulation.Bodies[hitHandler.HitCollidable.BodyHandle];
                    var shape = body.Collidable.Shape;
                    bool headshot = false;
                    bool crit = false;
                    if (_poseCompoundToAssetId.ContainsKey(shape))
                    {
                        var poseId = _poseCompoundToAssetId[shape];
                        var poseData = _assetIdToPoseCompoundData[poseId];
                        var poseShapeData = poseData[hitHandler.ChildIndex];
                        var physicsMaterial = SDBInterface.GetPhysicsMaterial((uint)poseShapeData.Material); // TODO: Material can be 0 which will result in null here, but what should we do? Is there a default to fallback to?

                        headshot = poseShapeData.ShapeFlags.Headshot;
                        crit = physicsMaterial?.IsCritHit == 1;
                        var damageMod = poseShapeData.DamageMod;

                        _logger.Debug($"ProjectileRayCast Impact on {poseShapeData.Name}");
                        _logger.Debug($"You hit {poseShapeData.Name} of {hitEntityId}");
                        if (source.IsPlayerControlled)
                        {
                            _eventBus.Enqueue(new DebugChatDirectMessageEvent($"You hit {poseShapeData.Name} of {hitEntityId}", source.Player));
                        }
                    }
                }
            }
        }
        else
        {
        }
    }

    public (bool, Vector3, ulong) TargetRayCast(Vector3 origin, Vector3 direction, CharacterEntity source, float maxRange = 500f)
    {
        bool outHit = false;
        Vector3 outPos = Vector3.Zero;
        ulong outEnt = 0;

        var hitHandler = default(RayHitHandler);
        hitHandler.T = maxRange;
        hitHandler.AvoidSourceBody = true;
        hitHandler.SourceBody = _entityIdToBody[source.EntityId];
        Simulation.RayCast(origin, direction, float.MaxValue, BufferPool, ref hitHandler);
        if (hitHandler.T < maxRange)
        {
            outHit = true;
            outPos = origin + (direction * hitHandler.T);
            outEnt = _bodyToEntityId[hitHandler.HitCollidable.BodyHandle];
        }

        return (outHit, outPos, outEnt);
    }

    partial void DebugInitialize(bool isDebugPipeClient, uint zoneId);

    private BodyDescription CreateTestBall(Vector3 pos)
    {
        var bulletShape = new Sphere(3f);
        var bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletShape.ComputeInertia(100), new(Simulation.Shapes.Add(bulletShape), 0.1f), 0.01f);
        bulletDescription.Pose.Position = pos;
        Simulation.Bodies.Add(bulletDescription);
        return bulletDescription;
    }

    private struct RayHitHandler : IRayHitHandler
    {
        public float T;
        public CollidableReference HitCollidable;
        public bool AvoidSourceBody;
        public BodyHandle SourceBody;
        public Vector3 Normal;
        public int ChildIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            if (AvoidSourceBody && collidable.Mobility != CollidableMobility.Static && collidable.BodyHandle.Equals(SourceBody))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            if (AvoidSourceBody && collidable.Mobility != CollidableMobility.Static && collidable.BodyHandle.Equals(SourceBody))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            // We are only interested in the earliest hit. This callback is executing within the traversal, so modifying maximumT informs the traversal
            // that it can skip any AABBs which are more distant than the new maximumT.
            maximumT = t;

            // Cache the earliest impact.
            T = t;
            HitCollidable = collidable;
            Normal = normal;
            ChildIndex = childIndex;
        }
    }
}