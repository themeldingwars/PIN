using System.Threading;

namespace GameServer;

public class PhysicsEngine
{
    public PhysicsEngine(double deltaTime)
    {
        Accumulator = 0.0;
        SimulatedTime = 0.0;
        DeltaTime = deltaTime;
    }

    public double Accumulator { get; protected set; }
    public double SimulatedTime { get; protected set; }
    public double DeltaTime { get; }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        Accumulator += deltaTime;

        while (!ct.IsCancellationRequested && Accumulator >= DeltaTime)
        {
            Integrate(/*state*/null, SimulatedTime, DeltaTime);
            Accumulator -= DeltaTime;
            SimulatedTime += DeltaTime;
        }
    }

    protected void Integrate(object state, double t, double dt)
    {
    }
}