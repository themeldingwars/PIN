using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using AeroMessages.GSS.V66.Character.Event;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using GameServer.Entities.Character;

namespace GameServer.Physics;

public class PhysicsEngine
{    
    public const float TargetTimestepDuration = 50; // 1 / 20f

    private Shard Shard;

    private TypedIndex DefaultCharacterShape;

    public PhysicsEngine(Shard shard)
    {
        Shard = shard;

        BufferPool = new BufferPool();

        // var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        var targetThreadCount = 2;
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));
        
        DefaultCharacterShape = Simulation.Shapes.Add(new Sphere(0.9f)); // Simulation.Shapes.Add(new Capsule(0.5f, 1.8f));

        new ZoneLoader.ZoneLoader(Simulation, BufferPool, ThreadDispatcher).LoadCollision(shard.ZoneId);
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
        return Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, DefaultCharacterShape, -1));
    }

    public void UpdateEntity(CharacterEntity entity)
    {
        ref var currentPose = ref Simulation.Bodies[entity.BodyHandle].Pose;
        currentPose.Position = entity.Position;
        currentPose.Position.Z += 0.9f;
        currentPose.Orientation = entity.Rotation;

        // QuaternionEx.Add(QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, 90f), entity.Rotation, out currentPose.Orientation);
    }

    public BodyDescription CreateTestBall(Vector3 pos)
    {
        var bulletShape = new Sphere(3f);
        var bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletShape.ComputeInertia(100), new(Simulation.Shapes.Add(bulletShape), 0.1f), 0.01f);
        bulletDescription.Pose.Position = pos;
        Simulation.Bodies.Add(bulletDescription);
        return bulletDescription;
    }

    public void CreateTestRayCast(Vector3 pos, Vector3 dir)
    {
        Console.WriteLine($"CreateTestRayCast from {pos} towards {dir}");

        var hitHandler = default(RayHitHandler);
        hitHandler.T = float.MaxValue;
        
        Simulation.RayCast(pos, dir, float.MaxValue, ref hitHandler);
        if (hitHandler.T < float.MaxValue)
        {   
            var posEst = pos + (dir * hitHandler.T);
            Console.WriteLine($"HitHandler {hitHandler.HitCollidable.Mobility} T {hitHandler.T} HitCollidable {hitHandler.HitCollidable} at {posEst}");
            if (hitHandler.HitCollidable.Mobility == CollidableMobility.Static)
            {
                Console.WriteLine($"Static Origin At {Simulation.Statics.GetStaticReference(hitHandler.HitCollidable.StaticHandle).Pose.Position}");
            }
        }
        else
        {
            Console.WriteLine($"Nothing hit");
        }
    }

    public void SendDebugProjectileSpawn(CharacterEntity source, uint traceId, Vector3 origin, Vector3 direction, float speed)
    {
        var rayVector = direction * speed;
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Spawn,
                Unk2_TraceId = traceId,
                Position = origin,
                Direction = rayVector,
            }
        };
        if (source.IsPlayerControlled)
        {
            Console.WriteLine($"SendDebugProjectileSpawn");
            source.Player.NetChannels[ChannelType.ReliableGss].SendIAero(msg, source.EntityId);
        }
    }

    public void SendDebugProjectileImpact(CharacterEntity source, uint traceId, Vector3 position, Vector3 normal)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Impact,
                Unk2_TraceId = traceId,
                Position = position,
                Direction = normal,
            }
        };
        if (source.IsPlayerControlled)
        {
            Console.WriteLine($"SendDebugProjectileImpact");
            source.Player.NetChannels[ChannelType.ReliableGss].SendIAero(msg, source.EntityId);
        }
    }

    public void SendDebugProjectilePoseHit(CharacterEntity source, uint traceId, Vector3 markerOrigin, Vector3 poseOrigin)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Posefile_Hit,
                Unk2_TraceId = traceId,
                Position = markerOrigin,
                Direction = new Vector3(0.225f, 0.974f, 0),
                HaveUnk8 = 1,
                Unk8 = new AeroMessages.GSS.V66.TookDebugWeaponHitRelatedData {
                    Target = source.AeroEntityId,
                    Unk2 = poseOrigin,
                    Unk3 = Quaternion.Identity,
                    Unk4 = 0,
                    Unk5 = 0xFF,
                },
                HaveUnk9 = 0,
            }
        };
        if (source.IsPlayerControlled)
        {
            Console.WriteLine($"SendDebugProjectilePoseHit");
            source.Player.NetChannels[ChannelType.ReliableGss].SendIAero(msg, source.EntityId);
        }
    }

    public void CreateTestFireRayCast(CharacterEntity source, Vector3 direction)
    {
        var projectileOffset = new Vector3(0.2f, 0.1f, 0f);
        var projectileBase = new Vector3(0f, 0f, 1.62f);
        var muzzleOffset = projectileBase + projectileOffset;
        var invQ = QuaternionEx.Inverse(source.Rotation);
        var origin = source.Position + QuaternionEx.Transform(muzzleOffset, invQ);
        
        uint traceId = 11111;
        var speed = 500f;
        var maxRange = 500f;
        var hitHandler = default(RayHitHandlerFire);
        hitHandler.T = maxRange;
        hitHandler.AvoidSourceBody = true;
        hitHandler.SourceBody = source.BodyHandle;

        SendDebugProjectileSpawn(source, traceId, origin, direction, speed);
        Simulation.RayCast(origin, direction, float.MaxValue, ref hitHandler);
        if (hitHandler.T < maxRange)
        {   
            var hitPosition = origin + (direction * hitHandler.T);
            Console.WriteLine($"HitHandler {hitHandler.HitCollidable.Mobility} T {hitHandler.T} HitCollidable {hitHandler.HitCollidable} at {hitPosition}");

            SendDebugProjectileImpact(source, traceId, hitPosition, hitHandler.Normal);

            if (hitHandler.HitCollidable.Mobility == CollidableMobility.Kinematic)
            {
                var bodyPosition = Simulation.Bodies[hitHandler.HitCollidable.BodyHandle].Pose.Position;
                bodyPosition.Z -= 0.9f;
                SendDebugProjectilePoseHit(source, traceId, hitPosition, bodyPosition);
            }

        }
        else
        {
            Console.WriteLine($"Nothing hit");
        }
    }

    public struct RayHitHandlerFire : IRayHitHandler
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

    public struct RayHitHandler : IRayHitHandler
    {
        public float T;
        public CollidableReference HitCollidable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
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
        }
    }
}