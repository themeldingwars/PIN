using Shared.Udp;

namespace MyGameServer.Packets.Common
{
    public class Quaternion
    {
        [Field] public float W;

        [Field] public float X;

        [Field] public float Y;

        [Field] public float Z;

        public static implicit operator System.Numerics.Quaternion(Quaternion q)
        {
            return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static implicit operator Quaternion(System.Numerics.Quaternion q)
        {
            return new Quaternion { X = q.X, Y = q.Y, Z = q.Z, W = q.W };
        }
    }
}