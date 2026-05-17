using System.Numerics;
using GameServer.Data.SDB.Records.dbcharacter;

namespace GameServer.Entities;

public class CollisionComponent
{
    public uint HitboxCollisionId;
    public float Scale = 1f;
}

public class CharacterCollisionComponent : CollisionComponent
{
    public PoseType PoseTypeRecord;
    public bool RequiresRagdoll;
    public uint RagdollCollisionId;
    public uint AttachmentPoseId;
    public Vector3 AttachmentPoseOffset;
}