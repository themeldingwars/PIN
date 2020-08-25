using System;

namespace GameServer.Packets
{
    public class UnknownPacketException : Exception
    {
        public UnknownPacketException()
            : base($"Encountered unknown packet type")
        {
        }
    }
}