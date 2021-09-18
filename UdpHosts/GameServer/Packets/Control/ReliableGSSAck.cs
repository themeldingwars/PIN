using GameServer.Enums;
using Shared.Udp;

namespace GameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.ReliableGSSAck)]
    public class ReliableGSSAck
    {
        [Field]
        public ushort NextSeqNum;

        [Field]
        public ushort AckFor;
    }
}