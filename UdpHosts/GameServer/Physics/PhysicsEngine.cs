using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using AeroMessages.GSS.V66.Character.Event;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using GameServer.Entities;
using GameServer.Entities.Character;
using Serilog;

namespace GameServer.Physics;

public class PhysicsEngine
{    
    public const float TargetTimestepDuration = 50; // (1/20f)

    private Shard _shard;
    private ILogger _logger;
    private TypedIndex _defaultCharacterShape;
    private Dictionary<BodyHandle, ulong> _bodyToEntityId = new();

    public PhysicsEngine(Shard shard)
    {
        _shard = shard;
        _logger = shard.Logger;

        // Determine number of threads to use
        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);

        // Setup BepuPhysics
        BufferPool = new BufferPool();
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));
        
        // Default shapes
        _defaultCharacterShape = Simulation.Shapes.Add(new Sphere(0.9f));
        
        // Load zone
        if (_shard.Settings.LoadMapsCollision)
        {
            new ZoneLoader.ZoneLoader(Simulation, BufferPool, ThreadDispatcher).LoadCollision(_shard.Settings.MapsPath, _shard.ZoneId);
        }
    }

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }
    public ThreadDispatcher ThreadDispatcher { get; private set; }
    public double TimeAccumulator { get; protected set; }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        TimeAccumulator += deltaTime;
        while (!ct.IsCancellationRequested && TimeAccumulator >= TargetTimestepDuration)
        {
            Simulation.Timestep(TargetTimestepDuration, ThreadDispatcher);
            TimeAccumulator -= TargetTimestepDuration;
        }
    }

    public BodyHandle CreateKineticEntity(CharacterEntity entity)
    {
        var pose = new RigidPose { Position = entity.Position, Orientation = entity.Rotation };
        var body = Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, _defaultCharacterShape, -1));
        _bodyToEntityId[body] = entity.EntityId;
        return body;
    }

    public void UpdateEntity(CharacterEntity entity)
    {
        ref var currentPose = ref Simulation.Bodies[entity.BodyHandle].Pose;
        currentPose.Position = entity.Position;
        currentPose.Position.Z += 0.9f;
        currentPose.Orientation = entity.Rotation;
    }

    public void ProjectileRayCast(Vector3 origin, Vector3 direction, CharacterEntity source, uint trace)
    {
        var speed = 500f;
        var maxRange = 500f;
        
        SendDebugProjectileSpawn(source, trace, origin, direction, speed);

        var hitHandler = default(RayHitHandler);
        hitHandler.T = maxRange;
        hitHandler.AvoidSourceBody = true;
        hitHandler.SourceBody = source.BodyHandle;

        Simulation.RayCast(origin, direction, float.MaxValue, ref hitHandler);
        if (hitHandler.T < maxRange)
        {   
            var hitPosition = origin + (direction * hitHandler.T);
            Console.WriteLine($"HitHandler {hitHandler.HitCollidable.Mobility} T {hitHandler.T} HitCollidable {hitHandler.HitCollidable} at {hitPosition}");

            SendDebugProjectileImpact(source, trace, hitPosition, hitHandler.Normal);

            if (hitHandler.HitCollidable.Mobility == CollidableMobility.Kinematic)
            {
                var bodyPosition = Simulation.Bodies[hitHandler.HitCollidable.BodyHandle].Pose.Position;
                bodyPosition.Z -= 0.9f;
                SendDebugProjectilePoseHit(source, trace, hitPosition, bodyPosition);
            }
        }
        else
        {
            Console.WriteLine($"Nothing hit");
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
        hitHandler.SourceBody = source.BodyHandle;
        Simulation.RayCast(origin, direction, float.MaxValue, ref hitHandler);
        if (hitHandler.T < maxRange)
        {   
            outHit = true;
            outPos = origin + (direction * hitHandler.T);
            outEnt = _bodyToEntityId[hitHandler.HitCollidable.BodyHandle];
        }

        return (outHit, outPos, outEnt);
    }
    
    private BodyDescription CreateTestBall(Vector3 pos)
    {
        var bulletShape = new Sphere(3f);
        var bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletShape.ComputeInertia(100), new(Simulation.Shapes.Add(bulletShape), 0.1f), 0.01f);
        bulletDescription.Pose.Position = pos;
        Simulation.Bodies.Add(bulletDescription);
        return bulletDescription;
    }

    private void SendDebugProjectileSpawn(CharacterEntity source, uint traceId, Vector3 origin, Vector3 direction, float speed)
    {
        var rayVector = direction * speed;
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = _shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Spawn,
                Unk2_TraceId = traceId,
                Position = origin,
                Direction = rayVector,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            // Console.WriteLine($"SendDebugProjectileSpawn");
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }

    private void SendDebugProjectileImpact(CharacterEntity source, uint traceId, Vector3 position, Vector3 normal)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = _shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Impact,
                Unk2_TraceId = traceId,
                Position = position,
                Direction = normal,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            // Console.WriteLine($"SendDebugProjectileImpact");
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }

    private void SendDebugProjectilePoseHit(CharacterEntity source, uint traceId, Vector3 markerOrigin, Vector3 poseOrigin)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = _shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Posefile_Hit,
                Unk2_TraceId = traceId,
                Position = markerOrigin,
                Direction = new Vector3(0.225f, 0.974f, 0),
                HaveUnk8 = 1,
                Unk8 = new AeroMessages.GSS.V66.TookDebugWeaponHitRelatedData
                {
                    Target = source.AeroEntityId,
                    Unk2 = poseOrigin,
                    Unk3 = Quaternion.Identity,
                    Unk4 = 0,
                    Unk5 = 0xFF,
                },
                HaveUnk9 = 0,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            // Console.WriteLine($"SendDebugProjectilePoseHit");
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }

    private struct RayHitHandler : IRayHitHandler
    {
        public float T;
        public CollidableReference HitCollidable;
        public bool AvoidSourceBody;
        public BodyHandle SourceBody;
        public Vector3 Normal;

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
        }
    }
}