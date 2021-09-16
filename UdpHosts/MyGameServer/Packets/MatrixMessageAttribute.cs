using MyGameServer.Enums;
using System;

namespace MyGameServer.Packets
{
    public class MatrixMessageAttribute : Attribute
    {
        public MatrixMessageAttribute(MatrixPacketType mID)
        {
            MsgID = mID;
        }

        public MatrixPacketType MsgID { get; protected set; }
    }
}