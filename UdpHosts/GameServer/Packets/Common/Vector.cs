using Shared.Udp;
using System.Numerics;

namespace GameServer.Packets.Common
{
    public class Vector
    {
        [Field] public float X;

        [Field] public float Y;

        [Field] public float Z;

        public static implicit operator Vector3(Vector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static implicit operator Vector(Vector3 v)
        {
            return new Vector { X = v.X, Y = v.Y, Z = v.Z };
        }
    }
}