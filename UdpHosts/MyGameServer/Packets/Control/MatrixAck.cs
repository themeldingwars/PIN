using MyGameServer.Enums;
using Shared.Udp;

namespace MyGameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.MatrixAck)]
    public class MatrixAck
    {
        [Field] public ushort AckFor;

        [Field] public ushort NextSeqNum;
    }
}