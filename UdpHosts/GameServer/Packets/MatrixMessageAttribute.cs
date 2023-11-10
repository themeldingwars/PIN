using System;
using GameServer.Enums;

namespace GameServer.Packets;

public class MatrixMessageAttribute : Attribute
{
    public MatrixMessageAttribute(MatrixPacketType mID)
    {
        MsgID = mID;
    }

    public MatrixPacketType MsgID { get; protected set; }
}