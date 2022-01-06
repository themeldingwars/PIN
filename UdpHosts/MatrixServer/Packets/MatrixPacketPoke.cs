using Shared.Udp;
using System.Runtime.InteropServices;
using System.Text;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketPoke
{
    public readonly uint SocketID;
    private fixed byte type[4];

    public string Type
    {
        get
        {
            fixed (byte* t = type)
            {
                return Deserializer.ReadFixedString(t, 4);
            }
        }
        set
        {
            fixed (byte* t = type)
            {
                Serializer.WriteFixed(t, Encoding.ASCII.GetBytes(value.Substring(0, 4)));
            }
        }
    }

    public readonly uint ProtocolVersion;
}