﻿using Shared.Udp;
using System.Runtime.InteropServices;
using System.Text;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketHehe
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

    public readonly uint ClientSocketID;

    public MatrixPacketHehe(uint clientID)
    {
        SocketID = 0;
        ClientSocketID = Utils.SimpleFixEndianness(clientID);
        Type = "HEHE";
    }
}