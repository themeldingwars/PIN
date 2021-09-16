using System;

namespace MyGameServer.Packets
{
    public class MessageIDAttribute : Attribute
    {
        public MessageIDAttribute(byte idNum)
        {
            IDNumber = idNum;
        }

        public byte IDNumber { get; protected set; }
    }
}