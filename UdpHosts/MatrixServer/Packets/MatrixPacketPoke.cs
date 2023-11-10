﻿using System.Runtime.InteropServices;
using System.Text;
using Shared.Udp;

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
                Serializer.WriteFixed(t, Encoding.ASCII.GetBytes(value[..4]));
            }
        }
    }

    public readonly uint ProtocolVersion;
}