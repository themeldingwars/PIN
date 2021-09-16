using MyGameServer.Enums;
using System;

namespace MyGameServer.Packets
{
    public class ControlMessageAttribute : Attribute
    {
        public ControlMessageAttribute(ControlPacketType mID)
        {
            MsgID = mID;
        }

        public ControlPacketType MsgID { get; protected set; }
    }
}