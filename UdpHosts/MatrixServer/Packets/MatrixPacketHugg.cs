using System.Runtime.InteropServices;
using System.Text;
using Shared.Udp;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketHugg
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
                Serializer.WriteFixed(t, Encoding.ASCII.GetBytes(value[..4]));
            }
        }
    }

    public readonly ushort SequenceStart;
    public readonly ushort GameServerPort;

    public MatrixPacketHugg(ushort seqStart, ushort port)
    {
        SocketID = 0;
        SequenceStart = Utils.SimpleFixEndianness(seqStart);
        GameServerPort = Utils.SimpleFixEndianness(port);
        Type = "HUGG";
    }
}