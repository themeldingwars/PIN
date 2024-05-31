using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;

namespace GameServer.Physics;

public class PhysicsEngine
{    
    public const float TargetTimestepDuration = 50; // 1 / 20f

    public PhysicsEngine(uint zoneId)
    {
        BufferPool = new BufferPool();

        // var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        var targetThreadCount = 2;
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        new ZoneLoader.ZoneLoader(Simulation, BufferPool).LoadCollision(zoneId);
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
            // && hitHandler.HitCollidable.Mobility == CollidableMobility.Dynamic
            Console.WriteLine($"HitHandler T {hitHandler.T} HitCollidable {hitHandler.HitCollidable} at {Simulation.Statics.GetStaticReference(hitHandler.HitCollidable.StaticHandle).Pose.Position}");
        }
        else
        {
            Console.WriteLine($"Nothing hit");
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