using MyGameServer.Enums;
using Shared.Udp;

namespace MyGameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.ReliableGSSAck)]
    public class ReliableGSSAck
    {
        [Field] public ushort AckFor;

        [Field] public ushort NextSeqNum;
    }
}