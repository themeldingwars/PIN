using System;

namespace GameServer.Packets.Matrix
{
    public class UnknownMatrixPacketException : Exception
    {
        public UnknownMatrixPacketException(string packetType)
            : base($"Encountered unknown Matrix packet type '{packetType}'")
        { 
        }
    }
}
