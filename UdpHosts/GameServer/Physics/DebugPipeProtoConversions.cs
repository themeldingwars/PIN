using System.Numerics;
using BepuPhysics;
using DebugPipeProto;

public static class ProtoConversions
{
    public static Vector3 ToNumerics(this PipeVector3 v)
        => new(v.X, v.Y, v.Z);

    public static PipeVector3 ToProto(this Vector3 v)
        => new() { X = v.X, Y = v.Y, Z = v.Z };

    public static Quaternion ToNumerics(this PipeQuaternion q)
        => new(q.X, q.Y, q.Z, q.W);

    public static PipeQuaternion ToProto(this Quaternion q)
        => new() { X = q.X, Y = q.Y, Z = q.Z, W = q.W };

    public static RigidPose ToBepu(this PipeRigidPose p)
        => new()
        {
            Position = p.Position.ToNumerics(),
            Orientation = p.Orientation.ToNumerics()
        };

    public static PipeRigidPose ToProto(this RigidPose p)
        => new()
        {
            Position = p.Position.ToProto(),
            Orientation = p.Orientation.ToProto()
        };
}