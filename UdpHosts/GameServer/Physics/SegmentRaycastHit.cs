using System.Numerics;
using BepuPhysics.Collidables;

namespace GameServer.Physics;

public struct SegmentRaycastHit
{
    public bool Hit;
    public float T;
    public Vector3 HitPosition;
    public Vector3 Normal;
    public ulong HitEntityId;
    public int ChildIndex;
    public CollidableReference Collidable;
}
