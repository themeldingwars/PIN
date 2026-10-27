#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AeroMessages.GSS.Character;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB;
using GameServer.Systems.Combat;
using GameServer.Systems.MovementRelay;
using GameServer.Systems.SystemEvents;
using Serilog;
using Shared.Collision;
using Shared.Collision.ZoneLoading;

namespace GameServer.Physics;

/// <summary>
///    Runs physics simulations
/// </summary>
public partial class PhysicsEngine
{
    public const float TimeStepMs = 50;
    public const float TimeStepSeconds = TimeStepMs / 1000;

    /// <summary>
    ///     Volume query tuning: how many sides approximate a cone's end caps, the smallest distance worth
    ///     querying, the nudge a shape is swept by, and the root finding bounds handed to the sweep task. The
    ///     nudge exists only because a sweep is the sole shape-vs-shape query Bepu exposes; keeping it small
    ///     keeps the result a pure overlap test.
    /// </summary>
    private const int _coneBaseSegments = 12;
    private const float _queryMinDistance = 1e-3f;
    private const float _sweepNudgeDistance = 1e-3f;
    private const float _sweepMinimumProgression = 1e-3f;
    private const float _sweepConvergenceThreshold = 1e-4f;
    private const int _sweepMaximumIterations = 8;

    private static readonly Vector3 _gravityVector = new(0f, 0f, -8f);

    private readonly MovementRelay _movement;
    private readonly ILogger _logger;
    private readonly EventBus _eventBus;
    private readonly ZoneLoader _zoneLoader;
    private readonly RigidBodyLoader _rigidBodyLoader;
    private readonly Dictionary<BodyHandle, ulong> _bodyToEntityId = [];
    private readonly Dictionary<ulong, BodyHandle> _entityIdToBody = [];
    private readonly HashSet<ulong> _npcEntityIds = [];
    private readonly Dictionary<ulong, NpcMotorState> _npcMotors = [];
    private readonly PhysicsEngineSettings _settings;
    private readonly ConcurrentQueue<PhysicsCommand> _pendingPhysicsCommands = new();

    private readonly TypedIndex _fallbackShape;
    private int _simThreadId;

    public PhysicsEngine(PhysicsEngineSettings settings, EventBus eventBus, MovementRelay movement, DebugProjectileHitCallbacks? debugProjectileHitCallbacks = null)
    {
        _settings = settings;
        _eventBus = eventBus;
        _movement = movement;
        _logger = Log.Logger.ForContext<PhysicsEngine>();
        DebugProjectileHitCallbacks = debugProjectileHitCallbacks;

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        BufferPool = new BufferPool();
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        var narrowPhaseCallbacks = new NarrowPhaseCallbacks { Engine = this };
        Simulation = Simulation.Create(BufferPool, narrowPhaseCallbacks, new PoseIntegratorCallbacks(_gravityVector), new SolveDescription(8, 1));

        _fallbackShape = Simulation.Shapes.Add(new Sphere(0.9f));

        _zoneLoader = new ZoneLoader(Simulation, BufferPool, ThreadDispatcher, _settings.MapsPath, _settings.CachePath);
        _rigidBodyLoader = new RigidBodyLoader(Simulation, BufferPool, ThreadDispatcher, _settings.AssetDBPath, _settings.CachePath);
        PoseLoader = new PoseLoader.PoseLoader(_settings.AssetDBPath);

        if (_settings.EnablePhysicsDebugRenderer)
        {
            InitDebugRenderer();
        }

        if (_settings.LoadMapsCollision)
        {
            LoadZone(_settings.ZoneId);
        }
    }

    private enum PhysicsOp
    {
        CreateKineticCharacter,
        CreateDynamicCharacter,
        CreateKineticBase,
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

        DebugRenderExtractStatics();
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
        var tookTimestep = false;

        while (!ct.IsCancellationRequested && TimeAccumulator >= TimeStepMs)
        {
            ApplyNpcMotors(TimeStepSeconds);
            Simulation.Timestep(TimeStepSeconds, ThreadDispatcher);
            TimeAccumulator -= TimeStepMs;
            tookTimestep = true;

            DebugRenderExtractBodies();
            DebugRenderUpdateStaticStream();
        }

        // Physics is authoritative for dynamic bodies: push the integrated pose back onto the entities
        // (and record a movement sample) after the fixed-timestep loop, but only if we actually stepped.
        if (tookTimestep)
        {
            SyncDynamicBodies();
        }
    }

    public void CreateKineticEntity(CharacterEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.CreateKineticCharacter, entity));
    }

    public void CreateKineticEntity(BaseEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.CreateKineticBase, entity));
    }

    public void CreateDynamicEntity(CharacterEntity entity)
    {
        _pendingPhysicsCommands.Enqueue(new PhysicsCommand(PhysicsOp.CreateDynamicCharacter, entity));
    }

    public void RegisterNpcEntity(ulong entityId)
    {
        _npcEntityIds.Add(entityId);
        if (!_npcMotors.ContainsKey(entityId))
        {
            _npcMotors[entityId] = new NpcMotorState();
        }
    }

    public void UnregisterNpcEntity(ulong entityId)
    {
        _npcEntityIds.Remove(entityId);
        _npcMotors.Remove(entityId);
    }

    public bool IsNpcBody(BodyHandle body)
    {
        if (body.Value < 0 || !_bodyToEntityId.TryGetValue(body, out var entityId))
        {
            return false;
        }

        return _npcEntityIds.Contains(entityId);
    }

    public bool IsKinematicBody(ulong entityId)
    {
        return _entityIdToBody.TryGetValue(entityId, out var bodyHandle) && Simulation.Bodies[bodyHandle].Kinematic;
    }

    /// <summary>
    ///     Reads the integrated pose and velocity back from a dynamic body into the entity.
    ///     Goes through <see cref="CharacterEntity.SetPoseData" /> so the movement view and timestamps are
    ///     refreshed, not just the raw position/orientation fields. Only meaningful for dynamic bodies.
    /// </summary>
    /// <param name="entity">The character entity to receive the integrated pose.</param>
    /// <param name="shortTime">The shard short-time to stamp the resulting movement sample with.</param>
    /// <returns>True when a non-kinematic body was found and synced, otherwise false.</returns>
    public bool SyncEntityFromBody(CharacterEntity entity, ushort shortTime)
    {
        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return false;
        }

        var body = Simulation.Bodies[bodyHandle];
        if (body.Kinematic)
        {
            return false;
        }

        ref var pose = ref body.Pose;
        var poseData = new MovementPoseData
        {
            MovementType = MovementDataType.PosRotState | MovementDataType.Aim,
            PosRotState = new MovementPosRotState
            {
                Pos = pose.Position,
                Rot = Quaternion.Inverse(pose.Orientation),
                MovementState = entity.MovementState,
            },
            Velocity = body.Velocity.Linear,
            Aim = entity.AimDirection, // Body has no aim of its own; carry the entity's aim through so the sync does not clobber it.
        };
        entity.SetPoseData(poseData, shortTime);
        return true;
    }

    /// <summary>
    ///     Publishes a desired horizontal velocity for a dynamic NPC body. The physics engine applies a bounded
    ///     impulse toward this velocity during each fixed timestep, so gravity, contact response, and external
    ///     knockback impulses are not overwritten.
    /// </summary>
    /// <param name="entity">The dynamic NPC character entity to steer.</param>
    /// <param name="desiredHorizontalVelocity">The desired horizontal (X/Y) linear velocity.</param>
    /// <param name="maxAcceleration">The maximum acceleration used to bound the steering impulse; negative values are clamped to zero.</param>
    /// <param name="orientation">The entity's orientation, applied to the body's pose.</param>
    public void SetNpcSteering(CharacterEntity entity, Vector3 desiredHorizontalVelocity, float maxAcceleration, Quaternion orientation)
    {
        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return;
        }

        var body = Simulation.Bodies[bodyHandle];
        if (body.Kinematic)
        {
            return;
        }

        if (maxAcceleration < 0f)
        {
            maxAcceleration = 0f;
        }

        if (!_npcMotors.TryGetValue(entity.EntityId, out var motor))
        {
            motor = new NpcMotorState();
            _npcMotors[entity.EntityId] = motor;
        }

        motor.DesiredVelocity = new Vector3(desiredHorizontalVelocity.X, desiredHorizontalVelocity.Y, 0f);
        motor.MaxAcceleration = maxAcceleration;
        motor.Orientation = orientation;
        motor.Enabled = true;

        body.Velocity.Angular = Vector3.Zero;
        body.Pose.Orientation = Quaternion.Inverse(orientation);
        body.Awake = true;
    }

    /// <summary>
    ///     Brakes a dynamic NPC body toward zero horizontal velocity using a bounded impulse.
    /// </summary>
    /// <param name="entity">The dynamic NPC character entity to brake.</param>
    /// <param name="brakeAcceleration">The maximum braking acceleration used to bound the impulse; negative values are clamped to zero.</param>
    /// <param name="orientation">The entity's orientation, applied to the body's pose.</param>
    public void StopNpcSteering(CharacterEntity entity, float brakeAcceleration, Quaternion orientation)
    {
        SetNpcSteering(entity, Vector3.Zero, MathF.Max(0f, brakeAcceleration), orientation);
    }

    /// <summary>
    ///     Disables the AI velocity motor for a dynamic NPC body. The body remains fully dynamic and can sleep.
    /// </summary>
    /// <param name="entity">The dynamic NPC character entity whose motor is disabled.</param>
    public void DisableNpcSteering(CharacterEntity entity)
    {
        if (_npcMotors.TryGetValue(entity.EntityId, out var motor))
        {
            motor.Enabled = false;
        }
    }

    /// <summary>
    ///     Reads the current integrated linear velocity from a dynamic NPC body.
    /// </summary>
    /// <param name="entity">The character entity whose body velocity is read.</param>
    /// <returns>The body's current linear velocity, or <see cref="Vector3.Zero" /> if the entity has no body or the body is kinematic.</returns>
    public Vector3 GetNpcVelocity(CharacterEntity entity)
    {
        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return Vector3.Zero;
        }

        var body = Simulation.Bodies[bodyHandle];
        return body.Kinematic ? Vector3.Zero : body.Velocity.Linear;
    }

    /// <summary>
    ///     Probes downward for a ground surface beneath an NPC, ignoring the NPC's own body and other NPC bodies.
    /// </summary>
    /// <param name="entity">The character entity to probe beneath.</param>
    /// <param name="range">The downward distance to probe, in world units.</param>
    /// <param name="minNormalZ">The minimum dot product of the hit normal with the up (Z) axis; surfaces shallower than this are rejected.</param>
    /// <param name="hitPoint">When a ground surface is found, the world-space point of the hit.</param>
    /// <param name="normal">When a ground surface is found, the surface normal of the hit.</param>
    /// <returns>True when a sufficiently up-facing ground surface was found, otherwise false.</returns>
    public bool TryGroundProbe(CharacterEntity entity, float range, float minNormalZ, out Vector3 hitPoint, out Vector3 normal)
    {
        hitPoint = Vector3.Zero;
        normal = Vector3.Zero;

        if (range <= 0f)
        {
            return false;
        }

        var origin = entity.Position + new Vector3(0f, 0f, 0.05f);
        var end = entity.Position - new Vector3(0f, 0f, range);
        var hit = SegmentRayCast(origin, end, entity.EntityId, ignoreNpcs: true);
        if (!hit.Hit)
        {
            return false;
        }

        if (Vector3.Dot(hit.Normal, Vector3.UnitZ) < minNormalZ)
        {
            return false;
        }

        hitPoint = hit.HitPosition;
        normal = hit.Normal;
        return true;
    }

    /// <summary>
    ///     Applies a small bounded pose correction to a dynamic NPC body. Used for the biped ledge step-up assist.
    /// </summary>
    /// <param name="entity">The dynamic NPC character entity to nudge.</param>
    /// <param name="delta">The world-space offset added to the body's position; zero-length deltas are ignored.</param>
    public void NudgeNpcPosition(CharacterEntity entity, Vector3 delta)
    {
        if (delta.LengthSquared() <= 0f)
        {
            return;
        }

        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return;
        }

        var body = Simulation.Bodies[bodyHandle];
        if (body.Kinematic)
        {
            return;
        }

        body.Pose.Position += delta;
        body.Awake = true;
    }

    /// <summary>
    ///     Applies a knockback to an entity's collision body. Only dynamic (server-authoritative) bodies can be
    ///     pushed: player characters use kinematic bodies, which have no mass for an impulse to act on, so the
    ///     caller has to tell that client to move instead. While the push lasts the NPC steering motor stands
    ///     down, otherwise the AI would steer the body straight back to its desired velocity on the very next
    ///     tick and the knockback would never be visible.
    /// </summary>
    /// <param name="entity">The character entity to push.</param>
    /// <param name="velocity">The velocity the body should move at as a result of the push; a zero Z keeps the body's current fall speed.</param>
    /// <param name="durationSeconds">How long the steering motor yields to the push.</param>
    /// <returns>True when the body was pushed, false when the entity has no dynamic body.</returns>
    public bool ApplyPushVelocity(CharacterEntity entity, Vector3 velocity, float durationSeconds)
    {
        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return false;
        }

        var body = Simulation.Bodies[bodyHandle];
        if (body.Kinematic)
        {
            return false;
        }

        var current = body.Velocity.Linear;
        body.Velocity.Linear = new Vector3(velocity.X, velocity.Y, velocity.Z != 0f ? velocity.Z : current.Z);
        body.Awake = true;

        // The suppression window lives on the motor, so a body that has not been steered yet still needs one -
        // otherwise the push lands, and the first SetNpcSteering afterwards builds a fresh motor with no window
        // and cancels the knockback on the very next timestep.
        if (!_npcMotors.TryGetValue(entity.EntityId, out var motor))
        {
            motor = new NpcMotorState();
            _npcMotors[entity.EntityId] = motor;
        }

        motor.PushRemainingSeconds = MathF.Max(motor.PushRemainingSeconds, durationSeconds);

        _logger.Debug("ApplyPushVelocity {entityId} before={current} after={velocity} pushSeconds={pushSeconds}",
            entity.EntityId,
            current,
            body.Velocity.Linear,
            motor.PushRemainingSeconds);

        return true;
    }

    /// <summary>
    ///     Probes downward for the ground surface a short horizontal distance ahead of an NPC (along a direction) and
    ///     reports how that forward ground compares in height to the NPC's current position. A positive <paramref name="heightDelta" />
    ///     is a ledge to climb; a large negative one (or a miss) is a gap/drop. <paramref name="surfaceNormal" /> is the
    ///     forward surface normal, so callers can tell a walkable top (up-facing) from a wall face (sideways).
    ///     Ignores the NPC's own body and other NPCs.
    /// </summary>
    /// <param name="entity">The character entity to probe ahead of.</param>
    /// <param name="direction">The horizontal direction to probe toward (Z is ignored).</param>
    /// <param name="probeDistance">The horizontal distance ahead of the entity to probe at, in world units.</param>
    /// <param name="probeUp">The Z offset above the probe point where the downward ray begins.</param>
    /// <param name="probeDown">The Z offset below the probe point where the downward ray ends.</param>
    /// <param name="forwardGround">When a surface is found, the world-space point of the hit ahead of the entity.</param>
    /// <param name="heightDelta">When a surface is found, the height difference between the forward ground and the entity's position.</param>
    /// <param name="surfaceNormal">When a surface is found, the surface normal of the forward ground.</param>
    /// <returns>True when a surface was found ahead of the entity, otherwise false.</returns>
    public bool TryProbeAheadGround(CharacterEntity entity, Vector3 direction, float probeDistance, float probeUp, float probeDown, out Vector3 forwardGround, out float heightDelta, out Vector3 surfaceNormal)
    {
        forwardGround = Vector3.Zero;
        heightDelta = 0f;
        surfaceNormal = Vector3.Zero;
        var dir = new Vector3(direction.X, direction.Y, 0f);
        if (probeDistance <= 0f || dir.LengthSquared() <= 0.001f)
        {
            return false;
        }

        dir = Vector3.Normalize(dir);
        var ahead = entity.Position + (dir * probeDistance);
        var from = ahead + new Vector3(0f, 0f, probeUp);
        var to = ahead + new Vector3(0f, 0f, -probeDown);
        var hit = SegmentRayCast(from, to, entity.EntityId, ignoreNpcs: true);
        if (!hit.Hit)
        {
            return false;
        }

        forwardGround = hit.HitPosition;
        heightDelta = hit.HitPosition.Z - entity.Position.Z;
        surfaceNormal = hit.Normal;
        return true;
    }

    /// <summary>
    ///     Casts a ray from <paramref name="origin"/> along <paramref name="direction"/> and returns a point lifted
    ///     off the first hit surface whose normal is sufficiently Z-up (a flat, walkable surface). Used to place a
    ///     freshly spawned dynamic body on valid ground in front of the issuing player (along their aim) rather than
    ///     at the player's own position, which can sit at or below the surface and leave the body embedded in the
    ///     geometry (an embedded body produces a large contact-normal impulse that stalls the velocity motor and
    ///     makes the body slide).
    /// </summary>
    /// <param name="origin">The world-space point to cast the ray from (typically the issuing player's position).</param>
    /// <param name="direction">The direction to cast the ray toward (typically the player's aim).</param>
    /// <param name="range">The distance the ray casts, in world units.</param>
    /// <param name="minNormalZ">The minimum dot product of the hit normal with the up (Z) axis; shallower surfaces are rejected.</param>
    /// <param name="surfaceOffset">The distance along the hit normal the spawn point is lifted above the surface.</param>
    /// <param name="ignoreEntityId">The entity whose own body is excluded from the cast; zero to ignore none.</param>
    /// <param name="spawnPoint">When a suitable surface is found, the world-space spawn point lifted off it.</param>
    /// <returns>True when a flat, walkable surface was found, otherwise false.</returns>
    public bool TryFindSurfaceSpawn(Vector3 origin, Vector3 direction, float range, float minNormalZ, float surfaceOffset, ulong ignoreEntityId, out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.Zero;
        if (range <= 0f)
        {
            return false;
        }

        var dir = Vector3.Normalize(direction);
        if (float.IsNaN(dir.X) || float.IsNaN(dir.Y) || float.IsNaN(dir.Z))
        {
            return false;
        }

        var to = origin + (dir * range);
        var hit = SegmentRayCast(origin, to, ignoreEntityId);
        if (!hit.Hit)
        {
            return false;
        }

        if (Vector3.Dot(hit.Normal, Vector3.UnitZ) < minNormalZ)
        {
            return false;
        }

        var normal = hit.Normal.LengthSquared() > 0.001f ? Vector3.Normalize(hit.Normal) : Vector3.UnitZ;
        spawnPoint = hit.HitPosition + (normal * surfaceOffset);
        return true;
    }

    /// <summary>
    ///     Pushes the integrated pose back onto every dynamic body's owning character entity. Called after the fixed-timestep
    ///     loop (only when a step actually ran) so the server is the authoritative source of truth for dynamically-simulated
    ///     characters (e.g. NPCs). The entity lookup, movement-sample recording, and the scoped pose broadcast are handled by
    ///     the <see cref="MovementRelay" /> (which owns the shard's entity map and clock); this method only maps bodies to
    ///     entities and syncs the body pose onto the entity.
    /// </summary>
    public void SyncDynamicBodies()
    {
        var shortTime = _movement.GetShortTime();
        foreach (var pair in _bodyToEntityId)
        {
            var body = Simulation.Bodies[pair.Key];
            if (body.Kinematic)
            {
                continue;
            }

            if (!_movement.TryGetCharacter(pair.Value, out var character))
            {
                continue;
            }

            SyncEntityFromBody(character, shortTime);
            _movement.RelayDynamicCharacterPose(character, shortTime);
        }
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

    public SegmentRaycastHit SegmentRayCast(Vector3 from, Vector3 to, ulong ignoreEntityId, bool ignoreNpcs = false)
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
        hitHandler.IgnoreNpcs = ignoreNpcs;
        hitHandler.Engine = ignoreNpcs ? this : null;

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

    /// <summary>
    /// Drops a point onto the zone terrain under position. Returns null when no static collidable is in reach,
    /// which is what loot spawning uses to sit a drop on the ground.
    /// </summary>
    /// <param name="position">The world position to drop the probe from.</param>
    /// <param name="probeHeight">How far above the position the probe starts.</param>
    /// <param name="maxDepth">How far below the probe start it may reach.</param>
    /// <returns>The ground surface height, or null when no static collidable is within reach.</returns>
    public float? SampleGroundHeight(Vector3 position, float probeHeight = 50f, float maxDepth = 100f)
    {
        AssertSimulationThread();

        var origin = new Vector3(position.X, position.Y, position.Z + probeHeight);
        var handler = new StaticOnlyRayHitHandler { T = maxDepth };

        Simulation.RayCast(origin, -Vector3.UnitZ, maxDepth, BufferPool, ref handler);

        return handler.Hit ? origin.Z - handler.T : null;
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
            var targets = QuerySplashEntityIdsInSphere(hit.HitPosition, impactRadius, source.EntityId);
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

    /// <summary>
    ///     Finds every entity whose collision volume touches a sphere. Unlike the splash route, this hands a
    ///     real Sphere to Bepu's swept-shape query, so the narrowphase tests the actual collision shapes,
    ///     compounds included, rather than the boxes drawn around them.
    /// </summary>
    /// <param name="center">The sphere centre.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="ignoreEntityId">An entity to leave out of the results, typically the caster.</param>
    /// <returns>The ids of every entity the sphere touches.</returns>
    public List<ulong> QueryEntityIdsInSphere(Vector3 center, float radius, ulong ignoreEntityId)
    {
        AssertSimulationThread();

        var results = new List<ulong>();
        if (radius <= _queryMinDistance)
        {
            return results;
        }

        SweepShape(new Sphere(radius), new RigidPose(center, Quaternion.Identity), Vector3.Zero, ignoreEntityId, results);
        return results;
    }

    /// <summary>
    ///     Finds every entity whose collision volume intersects a cone (or frustum) swept out along an axis.
    ///     The volume is built as a convex hull and handed to Bepu's swept-shape query, so the broadphase
    ///     trees and the narrowphase sweep tasks do the shape tests against the real collision shapes,
    ///     compounds included. Testing entity origins instead is what makes a cone that starts at the
    ///     caster's eye miss a target standing right in front of them: their feet sit well below the apex,
    ///     and a cone growing out of a zero radius has no width to catch them with yet.
    /// </summary>
    /// <param name="origin">The cone apex, or the centre of the near cap when nearRadius is nonzero.</param>
    /// <param name="direction">The cone axis; normalized internally.</param>
    /// <param name="range">How far the cone runs along the axis from the near cap.</param>
    /// <param name="nearRadius">The cone's radius at the near cap; zero gives a point apex.</param>
    /// <param name="farRadius">The cone's radius at the far cap.</param>
    /// <param name="ignoreEntityId">An entity to leave out of the results, typically the caster.</param>
    /// <returns>The ids of every entity the cone touches.</returns>
    public List<ulong> QueryEntityIdsInCone(Vector3 origin, Vector3 direction, float range, float nearRadius, float farRadius, ulong ignoreEntityId)
    {
        AssertSimulationThread();

        var results = new List<ulong>();
        if (range <= _queryMinDistance || farRadius <= _queryMinDistance)
        {
            return results;
        }

        var directionLength = direction.Length();
        if (directionLength <= _queryMinDistance)
        {
            return results;
        }

        var axis = direction / directionLength;

        // Any vector not parallel to the axis works for building the cap rings.
        var reference = MathF.Abs(Vector3.Dot(axis, Vector3.UnitZ)) < 0.999f ? Vector3.UnitZ : Vector3.UnitX;
        var side = Vector3.Normalize(Vector3.Cross(axis, reference));
        var up = Vector3.Normalize(Vector3.Cross(axis, side));

        var points = new List<Vector3>((_coneBaseSegments * 2) + 1);
        if (nearRadius > _queryMinDistance)
        {
            AddConeRing(points, origin, side, up, nearRadius);
        }
        else
        {
            points.Add(origin);
        }

        AddConeRing(points, origin + (axis * range), side, up, farRadius);

        // The hull stores its points relative to its own center, so the pose has to carry that center back.
        // A cone only fails to produce a hull when it has no volume, which the range and radius guards above
        // rule out short of a degenerate regop result.
        if (!ConvexHullHelper.CreateShape(CollectionsMarshal.AsSpan(points), BufferPool, out var center, out var hull))
        {
            _logger.Debug("QueryEntityIdsInCone could not build a cone hull: range={range} nearRadius={nearRadius} farRadius={farRadius}", range, nearRadius, farRadius);
            return results;
        }

        var pose = new RigidPose(center, Quaternion.Identity);

        // A sweep is the only shape-vs-shape query Bepu exposes, so the "sweep" is a nudge: the hull is
        // already the whole cone, and the displacement is small enough to be irrelevant to the result.
        SweepShape(hull, pose, axis * _sweepNudgeDistance, ignoreEntityId, results);
        hull.Dispose(BufferPool);
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

        // The source's body may still wait in the command queue, then there's nothing to skip yet
        hitHandler.AvoidSourceBody = _entityIdToBody.TryGetValue(source.EntityId, out hitHandler.SourceBody);
        Simulation.RayCast(origin, direction, float.MaxValue, BufferPool, ref hitHandler);
        if (hitHandler.T < maxRange)
        {
            outHit = true;
            outPos = origin + (direction * hitHandler.T);
            outEnt = hitHandler.HitCollidable.Mobility != CollidableMobility.Static
                ? _bodyToEntityId.GetValueOrDefault(hitHandler.HitCollidable.BodyHandle)
                : 0;
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

    private static Vector3 ClampLength(Vector3 value, float maxLength)
    {
        var length = value.Length();
        if (length <= maxLength || length <= 0f)
        {
            return value;
        }

        return value / length * maxLength;
    }

    /// <summary>
    ///     Appends the points of a circle perpendicular to the cone axis, for the hull's end caps.
    /// </summary>
    /// <param name="points">The point list to append to.</param>
    /// <param name="center">The centre of the ring.</param>
    /// <param name="side">One of the two in-plane basis vectors.</param>
    /// <param name="up">The other in-plane basis vector.</param>
    /// <param name="radius">The ring radius.</param>
    private static void AddConeRing(List<Vector3> points, Vector3 center, Vector3 side, Vector3 up, float radius)
    {
        for (var segment = 0; segment < _coneBaseSegments; segment++)
        {
            var angle = segment * ((MathF.PI * 2f) / _coneBaseSegments);
            points.Add(center + (((side * MathF.Cos(angle)) + (up * MathF.Sin(angle))) * radius));
        }
    }

    /// <summary>
    ///     Splash damage's target gather: a broadphase AABB pass followed by a sphere-vs-bounds test. It stays
    ///     its own route rather than sharing <see cref="QueryEntityIdsInSphere" /> because a blast wants the
    ///     cheap conservative sweep over everything it might have touched, and the damage falloff already
    ///     forgives the corners where a body's bounds reach further than the body does.
    /// </summary>
    /// <param name="center">The blast centre.</param>
    /// <param name="radius">The blast radius.</param>
    /// <param name="ignoreEntityId">The entity at the centre of the blast, which takes its direct hit separately.</param>
    /// <returns>The ids of every entity whose bounds fall inside the blast.</returns>
    private List<ulong> QuerySplashEntityIdsInSphere(Vector3 center, float radius, ulong ignoreEntityId)
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

    /// <summary>
    ///     Runs a volume query through Bepu's swept-shape path: the broadphase trees narrow the candidates by
    ///     their cached bounds, then the narrowphase sweep task for the shape pair tests the real shapes.
    /// </summary>
    /// <typeparam name="TShape">The Bepu shape describing the query volume.</typeparam>
    /// <param name="shape">The query volume.</param>
    /// <param name="pose">Where the volume sits.</param>
    /// <param name="nudge">How far the volume is swept, kept small enough to stay an overlap test.</param>
    /// <param name="ignoreEntityId">An entity to leave out of the results.</param>
    /// <param name="results">The list that touched entity ids are appended to.</param>
    private void SweepShape<TShape>(TShape shape, in RigidPose pose, Vector3 nudge, ulong ignoreEntityId, List<ulong> results)
        where TShape : unmanaged, IConvexShape
    {
        var velocity = new BodyVelocity(nudge, Vector3.Zero);
        var handler = new SweepEntityHitHandler
        {
            Engine = this,
            IgnoreEntityId = ignoreEntityId,
            Results = results,
            Seen = new HashSet<ulong>(),
        };

        Simulation.Sweep(shape, pose, velocity, 1f, BufferPool, ref handler, _sweepMinimumProgression, _sweepConvergenceThreshold, _sweepMaximumIterations);
    }

    private void DrainPending()
    {
        while (_pendingPhysicsCommands.TryDequeue(out var command))
        {
            switch (command.Op)
            {
                case PhysicsOp.CreateKineticCharacter:
                    ApplyCreateKinetic((CharacterEntity)command.Entity);
                    break;
                case PhysicsOp.CreateDynamicCharacter:
                    ApplyCreateDynamic((CharacterEntity)command.Entity);
                    break;
                case PhysicsOp.CreateKineticBase:
                    ApplyCreateKinetic((BaseEntity)command.Entity);
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

    private void ApplyCreateKinetic(CharacterEntity entity)
    {
        AssertSimulationThread();
        _logger.Debug("CreateKineticEntity Character {entityId}", entity.EntityId);
        var pose = new RigidPose { Position = entity.Position, Orientation = ToBodyOrientation(entity.Orientation) };
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);
        var body = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, shape, -1));
        _bodyToEntityId[body] = entity.EntityId;
        _entityIdToBody[entity.EntityId] = body;
    }

    private void ApplyCreateDynamic(CharacterEntity entity)
    {
        AssertSimulationThread();
        float mass = entity.GetMass;
        _logger.Debug("CreateDynamicEntity Character {entityId} mass={mass}", entity.EntityId, mass);
        var pose = new RigidPose { Position = entity.Position, Orientation = ToBodyOrientation(entity.Orientation) };
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);
        var inertia = ComputeNpcInertia(mass);
        var body = Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, inertia, new CollidableDescription(shape, 0.1f), 0.1f));
        _bodyToEntityId[body] = entity.EntityId;
        _entityIdToBody[entity.EntityId] = body;
    }

    private void ApplyCreateKinetic(BaseEntity entity)
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
    }

    private void ApplyUpdate(CharacterEntity entity)
    {
        AssertSimulationThread();

        if (!_entityIdToBody.TryGetValue(entity.EntityId, out var bodyHandle))
        {
            return;
        }

        var body = Simulation.Bodies[bodyHandle];

        // Dynamic NPC bodies are integrated by the engine and steered via velocity; do not teleport their pose.
        // (Players are kinematic and keep the existing relay behavior below.)
        if (!body.Kinematic)
        {
            return;
        }

        ref var currentPose = ref body.Pose;
        var currentShape = body.Collidable.Shape;
        AssetCompoundKey key = GetCharacterPoseAsset(entity);
        var shape = GetAssetShape(key);

        var orientation = Quaternion.Inverse(entity.Orientation);
        if (currentPose.Position != entity.Position || currentPose.Orientation != orientation || currentShape != shape)
        {
            body.Awake = true;
            body.SetShape(shape);
            currentPose.Position = entity.Position;
            currentPose.Orientation = orientation;
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

        _entityIdToBody.Remove(entity.EntityId);
        _bodyToEntityId.Remove(bodyHandle);
        _npcEntityIds.Remove(entity.EntityId);
        _npcMotors.Remove(entity.EntityId);
        Simulation.Bodies.Remove(bodyHandle);
    }

    private bool TryEmitDirectHit(CharacterEntity source, uint trace, CollidableReference collidable, int childIndex, Vector3 hitPosition, ulong hitEntityId, int damageAmount, byte damageType, bool isAbilityProjectile)
    {
        AssertSimulationThread();

        // Both players (kinematic) and NPCs (dynamic) use pose-based hit detection.
        if (collidable.Mobility != CollidableMobility.Kinematic && collidable.Mobility != CollidableMobility.Dynamic)
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

    /// <summary>
    ///     Inertia for a steered NPC body. Keeps the translational mass (so gravity and contact response push it)
    ///     but zeroes the inverse inertia tensor, giving the body infinite rotational inertia. The result is a
    ///     body that can translate and fall but can never tip over or acquire angular momentum from ground
    ///     contact — it is only ever rotated explicitly via <see cref="SetNpcSteering" />.
    /// </summary>
    /// <param name="mass">The body's translational mass.</param>
    /// <returns>The inertia for a steered NPC body (finite mass, infinite rotational inertia).</returns>
    private BodyInertia ComputeNpcInertia(float mass)
    {
        return new BodyInertia { InverseMass = mass > 0f ? 1f / mass : 0f };
    }

    /// <summary>
    ///     Applies the stored AI velocity motors for one fixed physics timestep. Only the horizontal X/Y velocity
    ///     is corrected; Z velocity is left to gravity, contacts, and external impulses.
    /// </summary>
    /// <param name="dt">The fixed physics timestep, in seconds.</param>
    private void ApplyNpcMotors(float dt)
    {
        foreach (var pair in _npcMotors)
        {
            if (!pair.Value.Enabled)
            {
                continue;
            }

            if (!_entityIdToBody.TryGetValue(pair.Key, out var bodyHandle))
            {
                continue;
            }

            var body = Simulation.Bodies[bodyHandle];
            if (body.Kinematic)
            {
                continue;
            }

            // A body that was just pushed keeps its knockback velocity for a while instead of having the
            // motor steer it straight back to the AI's desired velocity.
            if (pair.Value.PushRemainingSeconds > 0f)
            {
                pair.Value.PushRemainingSeconds -= dt;
                if (pair.Value.PushRemainingSeconds <= 0f)
                {
                    _logger.Debug("NpcMotor {entityId} push window expired at velocity={Velocity}, resuming toward desired={Desired} maxAcceleration={MaxAcceleration}",
                        pair.Key,
                        body.Velocity.Linear,
                        pair.Value.DesiredVelocity,
                        pair.Value.MaxAcceleration);
                }
            }
            else if (pair.Value.MaxAcceleration > 0f)
            {
                var current = body.Velocity.Linear;
                var desired = new Vector3(pair.Value.DesiredVelocity.X, pair.Value.DesiredVelocity.Y, current.Z);
                var deltaVelocity = desired - current;
                var maxDeltaVelocity = pair.Value.MaxAcceleration * dt;
                var inverseMass = body.LocalInertia.InverseMass;
                if (inverseMass > 0f)
                {
                    var impulse = ClampLength(deltaVelocity, maxDeltaVelocity) / inverseMass;
                    body.ApplyLinearImpulse(impulse);
                }
            }

            body.Velocity.Angular = Vector3.Zero;
            body.Pose.Orientation = Quaternion.Inverse(pair.Value.Orientation);
            body.Awake = true;
        }
    }

    partial void DebugInitialize(bool isDebugPipeClient, uint zoneId);

    partial void InitDebugRenderer();

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
        public bool IgnoreNpcs;
        public PhysicsEngine? Engine;
        public Vector3 Normal;
        public int ChildIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable)
        {
            if (collidable.Mobility != CollidableMobility.Static)
            {
                var bodyHandle = collidable.BodyHandle;
                if (AvoidSourceBody && bodyHandle.Equals(SourceBody))
                {
                    return false;
                }

                if (IgnoreNpcs && Engine is { } engine && engine.IsNpcBody(bodyHandle))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable, int childIndex)
        {
            if (collidable.Mobility != CollidableMobility.Static)
            {
                var bodyHandle = collidable.BodyHandle;
                if (AvoidSourceBody && bodyHandle.Equals(SourceBody))
                {
                    return false;
                }

                if (IgnoreNpcs && Engine is { } engine && engine.IsNpcBody(bodyHandle))
                {
                    return false;
                }
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

    private struct StaticOnlyRayHitHandler : IRayHitHandler
    {
        public float T;
        public bool Hit;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable)
            => collidable.Mobility == CollidableMobility.Static;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable, int childIndex)
            => collidable.Mobility == CollidableMobility.Static;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            maximumT = t;
            T = t;
            Hit = true;
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

    /// <summary>
    ///     Collects the entities touched by the swept volume queries. Statics are skipped because they are
    ///     level geometry rather than targets, and an entity is reported once however many of its compound
    ///     children the volume happens to cross.
    /// </summary>
    private struct SweepEntityHitHandler : ISweepHitHandler
    {
        public PhysicsEngine Engine;
        public ulong IgnoreEntityId;
        public List<ulong> Results;
        public HashSet<ulong> Seen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable)
        {
            return collidable.Mobility != CollidableMobility.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return true;
        }

        public void OnHit(ref float maximumT, float t, Vector3 hitLocation, Vector3 hitNormal, CollidableReference collidable)
        {
            RecordHit(collidable);
        }

        public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
        {
            RecordHit(collidable);
        }

        private readonly void RecordHit(CollidableReference collidable)
        {
            if (!Engine._bodyToEntityId.TryGetValue(collidable.BodyHandle, out var entityId))
            {
                return;
            }

            if (entityId == IgnoreEntityId || !Seen.Add(entityId))
            {
                return;
            }

            Results.Add(entityId);
        }
    }

    private sealed class NpcMotorState
    {
        public Vector3 DesiredVelocity;
        public float MaxAcceleration;
        public Quaternion Orientation;
        public bool Enabled;

        /// <summary>
        ///     Seconds of external push still being honoured. While this runs the motor skips its velocity
        ///     correction so the AI does not instantly cancel out a knockback, but keeps driving the pose.
        /// </summary>
        public float PushRemainingSeconds;
    }
}
