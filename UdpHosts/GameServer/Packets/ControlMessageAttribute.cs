using GameServer.Enums;
using System;

namespace GameServer.Packets;

public class ControlMessageAttribute : Attribute
{
    public ControlMessageAttribute(ControlPacketType mID)
    {
        MsgID = mID;
    }

    public ControlPacketType MsgID { get; protected set; }
}