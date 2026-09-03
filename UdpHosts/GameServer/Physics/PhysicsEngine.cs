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
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB;
using GameServer.Systems.Combat;
using GameServer.Systems.SystemEvents;
using Serilog;
using Shared.Collision;
using Shared.Collision.ZoneLoading;

namespace GameServer.Physics;

/// <summary>
///    Runs physics simulations (primarily hit detection)
/// </summary>
public partial class PhysicsEngine
{
    public const float TargetTimestepDuration = 50; // (1/20f)
    public const float TargetDebugTickDuration = 200;

    private readonly ILogger _logger;
    private readonly EventBus _eventBus;
    private readonly ZoneLoader _zoneLoader;
    private readonly RigidBodyLoader _rigidBodyLoader;
    private readonly Dictionary<BodyHandle, ulong> _bodyToEntityId = [];
    private readonly Dictionary<ulong, BodyHandle> _entityIdToBody = [];
    private readonly Dictionary<ulong, AssetCompoundKey> _entityIdToAssetKey = [];
    private readonly string _mapsPath = string.Empty;
    private readonly string _cachePath = string.Empty;
    private readonly bool _forceReload;
    private readonly bool _isDebugPipeClient;

    private TypedIndex _fallbackShape;
    private int _debugEntityIndex = -1;
    private double _debugTimeAccumulator;

    public PhysicsEngine(EventBus eventBus, uint zoneId, string mapsPath = "", string assetDBPath = "", bool loadMapsCollision = false, DebugProjectileHitCallbacks? debugProjectileHitCallbacks = null, bool isDebugPipeClient = false, string cachePath = "", bool forceReload = false)
    {
        _eventBus = eventBus;
        _logger = Log.Logger.ForContext<PhysicsEngine>();
        _mapsPath = mapsPath;
        _cachePath = cachePath;
        _forceReload = forceReload;
        DebugProjectileHitCallbacks = debugProjectileHitCallbacks;

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        BufferPool = new BufferPool();
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        _fallbackShape = Simulation.Shapes.Add(new Sphere(0.9f));

        _zoneLoader = new ZoneLoader(Simulation, BufferPool, ThreadDispatcher, mapsPath, cachePath);
        _rigidBodyLoader = new RigidBodyLoader(Simulation, BufferPool, ThreadDispatcher, assetDBPath, cachePath);
        PoseLoader = new PoseLoader.PoseLoader(assetDBPath);

        _isDebugPipeClient = isDebugPipeClient;
        DebugInitialize(isDebugPipeClient, zoneId);

        if (loadMapsCollision)
        {
            LoadZone(zoneId);
        }
    }

    public long? ZoneFileTimestamp { get; private set; }

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }
    public ThreadDispatcher ThreadDispatcher { get; private set; }
    public double TimeAccumulator { get; protected set; }
    public PoseLoader.PoseLoader PoseLoader { get; private set; }
    private DebugProjectileHitCallbacks? DebugProjectileHitCallbacks { get; set; }

    public void LoadZone(uint zoneId)
    {
        var ts = _zoneLoader.LoadZone(zoneId, _forceReload);
        if (ts.HasValue)
        {
            ZoneFileTimestamp = ts.Value;
        }
    }

    public StaticDescription[] LoadRigidBody(string assetId)
    {
        return _rigidBodyLoader.Load(assetId);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        TimeAccumulator += deltaTime;
        while (!ct.IsCancellationRequested && TimeAccumulator >= TargetTimestepDuration)
        {
            DebugProcessMessages();
            Simulation.Timestep(TargetTimestepDuration, ThreadDispatcher);
            TimeAccumulator -= TargetTimestepDuration;
        }

        if (!ct.IsCancellationRequested && !_isDebugPipeClient)
        {
            _debugTimeAccumulator += deltaTime;
            if (_debugTimeAccumulator >= TargetDebugTickDuration)
            {
                DebugSendTickUpdate();
                _debugTimeAccumulator = 0;
            }
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

    public bool HasEntity(IEntity entity)
    {
        return _entityIdToBody.ContainsKey(entity.EntityId);
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

    /// <summary>
    ///     Probes the static zone geometry straight down and returns the
    ///     ground height at a position. Kinematic bodies (characters,
    ///     deployables) are ignored, so that an NPC below the probe cannot
    ///     appear as terrain. Returns null when the probe hits no static
    ///     geometry (zone collision not loaded, or outside the mesh).
    /// </summary>
    /// <param name="position">The probe position. X and Y select the column; Z is the reference height.</param>
    /// <param name="walkableEntityId">Optional. The surface of this kinematic entity also counts as ground. A climber can then walk on the hull of the entity.</param>
    /// <param name="probeUp">The probe start height above the position, in meters.</param>
    /// <param name="probeDown">The probe depth below the start, in meters.</param>
    public float? GetGroundHeight(Vector3 position, ulong walkableEntityId = 0, float probeUp = 40f, float probeDown = 120f)
    {
        var from = position + new Vector3(0f, 0f, probeUp);
        var distance = probeUp + probeDown;

        var hitHandler = default(RayHitHandler);
        hitHandler.T = distance;
        hitHandler.StaticsOnly = true;
        if (walkableEntityId != 0 && _entityIdToBody.TryGetValue(walkableEntityId, out var walkableBody))
        {
            hitHandler.HasWalkableBody = true;
            hitHandler.WalkableBody = walkableBody;
        }

        Simulation.RayCast(from, new Vector3(0f, 0f, -1f), distance, BufferPool, ref hitHandler);

        if (hitHandler.T < distance)
        {
            return from.Z - hitHandler.T;
        }

        return null;
    }

    public SegmentRaycastHit SegmentRayCast(Vector3 from, Vector3 to, ulong ignoreEntityId)
    {
        var hitResult = default(SegmentRaycastHit);
        var direction = Vector3.Normalize(to - from);
        var distance = Vector3.Distance(from, to);

        if (distance < 0.01f)
        {
            return hitResult;
        }

        var hitHandler = default(RayHitHandler);
        hitHandler.T = distance;
        hitHandler.AvoidSourceBody = ignoreEntityId != 0;
        hitHandler.SourceBody = _entityIdToBody.GetValueOrDefault(ignoreEntityId);

        Simulation.RayCast(from, direction, distance, BufferPool, ref hitHandler);

        if (hitHandler.T < distance)
        {
            hitResult.Hit = true;
            hitResult.T = hitHandler.T;
            hitResult.HitPosition = from + (direction * hitHandler.T);
            hitResult.Normal = hitHandler.Normal;
            hitResult.ChildIndex = hitHandler.ChildIndex;
            hitResult.Collidable = hitHandler.HitCollidable;

            // Only a body-owned collidable has a body handle. A read on a
            // static collidable (zone terrain, with LoadMapsCollision on)
            // stops the process with a Bepu assertion.
            hitResult.HitEntityId = hitHandler.HitCollidable.Mobility != CollidableMobility.Static
                ? _bodyToEntityId.GetValueOrDefault(hitHandler.HitCollidable.BodyHandle)
                : 0;
        }

        return hitResult;
    }

    public void HandleProjectileImpact(CharacterEntity source, uint trace, SegmentRaycastHit hit)
    {
        DebugProjectileHitCallbacks?.SendDebugProjectileImpact(source, trace, hit.HitPosition, hit.Normal);

        if (hit.Collidable.Mobility == CollidableMobility.Kinematic)
        {
            var bodyPosition = Simulation.Bodies[hit.Collidable.BodyHandle].Pose.Position;
            bodyPosition.Z -= 0.9f;
            DebugProjectileHitCallbacks?.SendDebugProjectilePoseHit(source, trace, hit.HitPosition, bodyPosition);

            var hitEntityId = _bodyToEntityId.GetValueOrDefault(hit.Collidable.BodyHandle);
            if (hitEntityId != 0 && TryGetActivePoseShapeData(hit.Collidable, hit.ChildIndex, out var poseShapeData))
            {
                var physicsMaterial = SDBInterface.GetPhysicsMaterial((uint)poseShapeData.Material);

                var headshot = poseShapeData.ShapeFlags.Headshot;
                var crit = physicsMaterial?.IsCritHit == 1;
                var damageMod = poseShapeData.DamageMod;

                _logger.Debug("ProjectileSim Impact on {ShapeName} (headshot={Headshot}, crit={Crit}, damageMod={DamageMod})", poseShapeData.Name, headshot, crit, damageMod);
                _logger.Debug("You hit {ShapeName} of {EntityId}", poseShapeData.Name, hitEntityId);
                _eventBus.Enqueue(new ProjectileHitEvent(hitEntityId, 1337, source.EntityId, headshot, crit, damageMod));
                if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon != 0)
                {
                    _eventBus.Enqueue(new DebugChatDirectMessageEvent($"You hit {poseShapeData.Name} of {hitEntityId}", source.Player));
                }
            }
        }
    }

    public bool TryGetActivePoseShapeData(CollidableReference collidable, int childIndex, out ActivePoseShapeData shapeData)
    {
        shapeData = default;

        var body = Simulation.Bodies[collidable.BodyHandle];
        var shape = body.Collidable.Shape;
        if (!_poseCompoundToAssetId.TryGetValue(shape, out var poseId))
        {
            return false;
        }

        if (!_assetIdToPoseCompoundData.TryGetValue(poseId, out var poseData))
        {
            return false;
        }

        return poseData.TryGetValue(childIndex, out shapeData);
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
        public bool StaticsOnly;
        public bool HasWalkableBody;
        public BodyHandle WalkableBody;
        public BodyHandle SourceBody;
        public Vector3 Normal;
        public int ChildIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable)
        {
            // StaticsOnly accepts static geometry, plus one optional walkable body
            if (StaticsOnly && collidable.Mobility != CollidableMobility.Static
                && !(HasWalkableBody && collidable.BodyHandle.Equals(WalkableBody)))
            {
                return false;
            }

            if (AvoidSourceBody && collidable.Mobility != CollidableMobility.Static && collidable.BodyHandle.Equals(SourceBody))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable, int childIndex)
        {
            if (StaticsOnly && collidable.Mobility != CollidableMobility.Static
                && !(HasWalkableBody && collidable.BodyHandle.Equals(WalkableBody)))
            {
                return false;
            }

            if (AvoidSourceBody && collidable.Mobility != CollidableMobility.Static && collidable.BodyHandle.Equals(SourceBody))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            maximumT = t;
            T = t;
            HitCollidable = collidable;
            Normal = normal;
            ChildIndex = childIndex;
        }
    }
}
