using System;
using System.Numerics;
using System.Threading;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;

namespace GameServer.Physics;

public class PhysicsEngine
{    
    public const float TargetTimestepDuration = 1 / 20f;

    public PhysicsEngine()
    {
        BufferPool = new BufferPool();

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));
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
}