using System.Numerics;
using GameServer.Enums;
using GameServer.StaticDB.Records.dbitems;

namespace GameServer.Systems.ProjectileSim;

public struct ActiveProjectile
{
    public ulong EntityId;
    public uint TraceId;
    public AmmoFlags.SimulationMode Type;
    public Ammo Ammo;

    public Vector3 Origin;
    public Vector3 Direction;
    public Vector3 Velocity;
    public Vector3 InitialVelocity;

    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Vector3 CurrentPosition;
    public Vector3 PreviousPosition;
    public Vector3 DrunkOffset;

    public uint StartTime;
    public uint LifetimeMs;

    public float Range;
    public byte BouncesRemaining;
    public byte HitsRemaining;
    public bool IsAlive;
    public bool HasHit;

    public ulong TargetEntityId;

    public ulong HitEntityId;
    public Vector3 HitPosition;
    public Vector3 HitNormal;

    public float AccumulatedDt;
    public float ImpactRadius;
    public float MaxRadius;

    public bool IsDrunk;
}
