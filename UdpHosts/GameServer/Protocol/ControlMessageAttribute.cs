using System;
using GameServer.Enums;

namespace GameServer.Protocol;

public class ControlMessageAttribute : Attribute
{
    public ControlMessageAttribute(ControlPacketType mID)
    {
        MsgID = mID;
    }

    public ControlPacketType MsgID { get; protected set; }
}