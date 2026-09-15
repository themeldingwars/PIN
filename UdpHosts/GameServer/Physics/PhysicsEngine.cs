#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly PhysicsEngineSettings _settings;
    private readonly ConcurrentQueue<PhysicsCommand> _pendingPhysicsCommands = new();

    private readonly TypedIndex _fallbackShape;
    private int _simThreadId;
    private int _debugEntityIndex = -1;
    private double _debugTimeAccumulator;

    public PhysicsEngine(PhysicsEngineSettings settings, EventBus eventBus, DebugProjectileHitCallbacks? debugProjectileHitCallbacks = null)
    {
        _settings = settings;
        _eventBus = eventBus;
        _logger = Log.Logger.ForContext<PhysicsEngine>();
        DebugProjectileHitCallbacks = debugProjectileHitCallbacks;

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        BufferPool = new BufferPool();
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        _fallbackShape = Simulation.Shapes.Add(new Sphere(0.9f));

        _zoneLoader = new ZoneLoader(Simulation, BufferPool, ThreadDispatcher, _settings.MapsPath, _settings.CachePath);
        _rigidBodyLoader = new RigidBodyLoader(Simulation, BufferPool, ThreadDispatcher, _settings.AssetDBPath, _settings.CachePath);
        PoseLoader = new PoseLoader.PoseLoader(_settings.AssetDBPath);

        if (_settings.EnableDebugPipe)
        {
            DebugInitialize(_settings.IsDebugPipeClient, _settings.ZoneId);
        }

        if (_settings.LoadMapsCollision)
        {
            LoadZone(_settings.ZoneId);
        }
    }

    private enum PhysicsOp
    {
        CreateCharacter,
        CreateBase,
        UpdateCharacter,
        UpdateBase,
        Remove,
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
        AssertSimulationThread();

        var ts = _zoneLoader.LoadZone(zoneId, _settings.ForceReload);
        if (ts.HasValue)
        {
            ZoneFileTimestamp = ts.Value;
        }
    }

    public StaticDescription[] LoadRigidBody(string assetId)
    {
        AssertSimulationThread();

        return _rigidBodyLoader.Load(assetId);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        _simThreadId = Environment.CurrentManagedThreadId;
        DrainPending();

        TimeAccumulator += deltaTime;
        while (!ct.IsCancellationRequested && TimeAccumulator >= TargetTimestepDuration)
        {
            DebugProcessMessages();
            Simulation.Timestep(TargetTimestepDuration, ThreadDispatcher);
            TimeAccumulator -= TargetTimestepDuration;
        }

        if (!ct.IsCancellationRequested && !_settings.IsDebugPipeClient)
        {
            _debugTimeAccumulator += deltaTime;
            if (_debugTimeAccumulator >= TargetDebugTickDuration)
            {
                DebugSendTickUpdate();
                _debugTimeAccumulator = 0;
            }
        }
    }

    public void CreateKineticEntity(CharacterEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.CreateCharacter, entity));
    }

    public void CreateKineticEntity(BaseEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.CreateBase, entity));
    }

    public void UpdateEntity(CharacterEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.UpdateCharacter, entity));
    }

    public void UpdateEntity(BaseEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.UpdateBase, entity));
    }

    public void RemoveEntity(IEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.Remove, entity));
    }

    public SegmentRaycastHit SegmentRayCast(Vector3 from, Vector3 to, ulong ignoreEntityId)
    {
        AssertSimulationThread();

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

    public void HandleProjectileImpact(CharacterEntity source, uint trace, SegmentRaycastHit hit, float impactRadius, float damage, byte damageType, bool isAbilityProjectile = false)
    {
        DebugProjectileHitCallbacks?.SendDebugProjectileImpact(source, trace, hit.HitPosition, hit.Normal);

        int damageAmount = Math.Max(0, (int)MathF.Round(damage));
        if (damageAmount <= 0)
        {
            return;
        }

        if (hit.HitEntityId != 0)
        {
            bool emittedDirectHit = TryEmitDirectHit(source, trace, hit.Collidable, hit.ChildIndex, hit.HitPosition, hit.HitEntityId, damageAmount, damageType, isAbilityProjectile);
            if (!emittedDirectHit)
            {
                EmitGenericProjectileHit(hit.HitEntityId, damageAmount, damageType, source.EntityId, isAbilityProjectile, isSplash: false);
            }
        }

        if (impactRadius > 0f)
        {
            var targets = QueryEntityIdsInSphere(hit.HitPosition, impactRadius, source.EntityId);
            foreach (var targetId in targets)
            {
                if (targetId == hit.HitEntityId)
                {
                    continue;
                }

                EmitGenericProjectileHit(targetId, damageAmount, damageType, source.EntityId, isAbilityProjectile, isSplash: true);
            }
        }
    }

    public List<ulong> QueryEntityIdsInSphere(Vector3 center, float radius, ulong ignoreEntityId)
    {
        AssertSimulationThread();

        var results = new List<ulong>();
        if (radius <= 0f)
        {
            return results;
        }

        bool hasIgnoreBody = _entityIdToBody.TryGetValue(ignoreEntityId, out var ignoreBody);
        var enumerator = new EntitySphereEnumerator
        {
            Center = center,
            Radius = radius,
            IgnoreEntityId = ignoreEntityId,
            IgnoreBody = ignoreBody,
            HasIgnoreBody = hasIgnoreBody,
            Engine = this,
            Results = results,
            Seen = new HashSet<ulong>()
        };

        Simulation.BroadPhase.GetOverlaps(center - new Vector3(radius), center + new Vector3(radius), BufferPool, ref enumerator);
        return results;
    }

    public bool TryGetActivePoseShapeData(CollidableReference collidable, int childIndex, out ActivePoseShapeData shapeData)
    {
        AssertSimulationThread();

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
        AssertSimulationThread();

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

    private static bool SphereIntersectsAabb(Vector3 center, float radius, Vector3 min, Vector3 max)
    {
        var closest = Vector3.Clamp(center, min, max);
        var delta = center - closest;
        return delta.LengthSquared() <= radius * radius;
    }

    private static Quaternion ToBodyOrientation(Quaternion orientation)
    {
        return Quaternion.Normalize(Quaternion.Inverse(orientation));
    }

    private void DrainPending()
    {
        while (_pendingPhysicsCommands.TryDequeue(out var command))
        {
            switch (command.Op)
            {
                case PhysicsOp.CreateCharacter:
                    ApplyCreate((CharacterEntity)command.Entity);
                    break;
                case PhysicsOp.CreateBase:
                    ApplyCreate((BaseEntity)command.Entity);
                    break;
                case PhysicsOp.UpdateCharacter:
                    ApplyUpdate((CharacterEntity)command.Entity);
                    break;
                case PhysicsOp.UpdateBase:
                    ApplyUpdate((BaseEntity)command.Entity);
                    break;
                case PhysicsOp.Remove:
                    ApplyRemove(command.Entity);
                    break;
            }
        }
    }

    [Conditional("DEBUG")]
    private void AssertSimulationThread([CallerMemberName] string caller = "")
    {
        if (_simThreadId != 0 && Environment.CurrentManagedThreadId != _simThreadId)
        {
            Debug.Fail($"{caller} touched the simulation from thread {Environment.CurrentManagedThreadId}, it belongs to the shard thread {_simThreadId}.");
        }
    }

    private void ApplyCreate(CharacterEntity entity)
    {
        AssertSimulationThread();
        _logger.Debug("CreateKineticEntity Character {entityId}", entity.EntityId);
        var pose = new RigidPose { Position = entity.Position, Orientation = ToBodyOrientation(entity.Orientation) };
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
    }

    private void ApplyCreate(BaseEntity entity)
    {
        AssertSimulationThread();
        _logger.Debug("CreateKineticEntity Base {entityId}", entity.EntityId);
        var assetId = entity.Collision.HitboxCollisionId;
        var offset = Vector3.Zero;
        var scale = entity.Collision.Scale;
        var pose = new RigidPose { Position = entity.Position, Orientation = ToBodyOrientation(entity.Orientation) };
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
    }

    private void ApplyUpdate(CharacterEntity entity)
    {
        AssertSimulationThread();

        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return;
        }

        var body = Simulation.Bodies[bodyHandle];
        var currentPose = body.Pose;
        var currentShape = body.Collidable.Shape;
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);

        var orientation = ToBodyOrientation(entity.Orientation);
        if (currentPose.Position != entity.Position || currentPose.Orientation != orientation || currentShape != shape)
        {
            _entityIdToAssetKey[entity.EntityId] = key;
            body.Awake = true;
            body.SetShape(shape);

            ref var pose = ref body.Pose;
            pose.Position = entity.Position;
            pose.Orientation = orientation;
        }
    }

    private void ApplyUpdate(BaseEntity entity)
    {
        AssertSimulationThread();

        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return;
        }

        var body = Simulation.Bodies[bodyHandle];
        var currentPose = body.Pose;

        var orientation = ToBodyOrientation(entity.Orientation);
        if (currentPose.Position != entity.Position || currentPose.Orientation != orientation)
        {
            body.Awake = true;

            ref var pose = ref body.Pose;
            pose.Position = entity.Position;
            pose.Orientation = orientation;
        }
    }

    private void ApplyRemove(IEntity entity)
    {
        AssertSimulationThread();

        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            // Every entity removal is queued, including the many that never had a body.
            _logger.Verbose("RemoveEntity for {entity} found no body", entity.ToString());
            return;
        }

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

    private bool TryEmitDirectHit(CharacterEntity source, uint trace, CollidableReference collidable, int childIndex, Vector3 hitPosition, ulong hitEntityId, int damageAmount, byte damageType, bool isAbilityProjectile)
    {
        AssertSimulationThread();

        if (collidable.Mobility != CollidableMobility.Kinematic)
        {
            return false;
        }

        var bodyPosition = Simulation.Bodies[collidable.BodyHandle].Pose.Position;
        bodyPosition.Z -= 0.9f;
        DebugProjectileHitCallbacks?.SendDebugProjectilePoseHit(source, trace, hitPosition, bodyPosition);

        if (hitEntityId == 0)
        {
            return false;
        }

        if (!TryGetActivePoseShapeData(collidable, childIndex, out var poseShapeData))
        {
            return false;
        }

        var physicsMaterial = SDBInterface.GetPhysicsMaterial((uint)poseShapeData.Material);

        var headshot = poseShapeData.ShapeFlags.Headshot;
        var crit = physicsMaterial?.IsCritHit == 1;
        var damageMod = poseShapeData.DamageMod;

        _logger.Debug("ProjectileSim Impact on {ShapeName} (headshot={Headshot}, crit={Crit}, damageMod={DamageMod})", poseShapeData.Name, headshot, crit, damageMod);
        _eventBus.Enqueue(new ProjectileHitEvent(poseShapeData.Name, hitEntityId, damageAmount, source.EntityId, headshot, crit, damageType, damageMod, isAbilityProjectile));
        return true;
    }

    private void EmitGenericProjectileHit(ulong targetId, int damageAmount, byte damageType, ulong sourceId, bool isAbilityProjectile, bool isSplash)
    {
        if (damageAmount <= 0)
        {
            return;
        }

        _eventBus.Enqueue(new ProjectileHitEvent("body", targetId, damageAmount, sourceId, false, false, damageType, -1f, isAbilityProjectile, isSplash));
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

    private readonly record struct PhysicsCommand(PhysicsOp Op, IEntity Entity);

    private struct RayHitHandler : IRayHitHandler
    {
        public float T;
        public CollidableReference HitCollidable;
        public bool AvoidSourceBody;
        public BodyHandle SourceBody;
        public Vector3 Normal;
        public int ChildIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable)
        {
            if (AvoidSourceBody && collidable.Mobility != CollidableMobility.Static && collidable.BodyHandle.Equals(SourceBody))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable, int childIndex)
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
            maximumT = t;
            T = t;
            HitCollidable = collidable;
            Normal = normal;
            ChildIndex = childIndex;
        }
    }

    private struct EntitySphereEnumerator : IBreakableForEach<CollidableReference>
    {
        public Vector3 Center;
        public float Radius;
        public ulong IgnoreEntityId;
        public BodyHandle IgnoreBody;
        public bool HasIgnoreBody;
        public PhysicsEngine Engine;
        public List<ulong> Results;
        public HashSet<ulong> Seen;

        public bool LoopBody(CollidableReference collidable)
        {
            if (collidable.Mobility == CollidableMobility.Static)
            {
                return true;
            }

            var bodyHandle = collidable.BodyHandle;
            if (HasIgnoreBody && bodyHandle.Equals(IgnoreBody))
            {
                return true;
            }

            if (!Engine._bodyToEntityId.TryGetValue(bodyHandle, out var entityId))
            {
                return true;
            }

            if (entityId == IgnoreEntityId || !Seen.Add(entityId))
            {
                return true;
            }

            var body = Engine.Simulation.Bodies[bodyHandle];
            var shape = body.Collidable.Shape;
            if (!shape.Exists)
            {
                return true;
            }

            Engine.Simulation.Shapes[shape.Type].ComputeBounds(shape.Index, body.Pose, out var min, out var max);
            if (SphereIntersectsAabb(Center, Radius, min, max))
            {
                Results.Add(entityId);
            }

            return true;
        }
    }
}
