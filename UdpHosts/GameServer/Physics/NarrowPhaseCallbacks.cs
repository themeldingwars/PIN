using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace GameServer.Physics;

#nullable enable
public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public NarrowPhaseCallbacks()
    {
    }

    public PhysicsEngine? Engine { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        // NPCs do not physically block one another; local avoidance is handled by the movement system (Crowd, Phase 2).
        if (Engine is { } engine &&
            a.Mobility == CollidableMobility.Dynamic && b.Mobility == CollidableMobility.Dynamic &&
            engine.IsNpcBody(a.BodyHandle) && engine.IsNpcBody(b.BodyHandle))
        {
            return false;
        }

        return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial)
        where TManifold : unmanaged, IContactManifold<TManifold>
    {
        // NPCs climb via a bounded horizontal velocity motor; low friction lets it beat gravity on inclines.
        var friction = 1f;
        if (Engine is { } engine &&
            ((engine.IsNpcBody(pair.A.BodyHandle) && pair.B.Mobility == CollidableMobility.Static) ||
             (engine.IsNpcBody(pair.B.BodyHandle) && pair.A.Mobility == CollidableMobility.Static)))
        {
            friction = 0.5f;
        }

        pairMaterial = new PairMaterialProperties { FrictionCoefficient = friction, MaximumRecoveryVelocity = 2, SpringSettings = new SpringSettings(30, 1) };
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }

    public readonly void Dispose()
    {
    }

    public readonly void Initialize(Simulation simulation)
    {
    }
}