using GameServer.Enums;
using Shared.Udp;

namespace GameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.MatrixAck)]
    public class MatrixAck
    {
        [Field] public ushort AckFor;

        [Field] public ushort NextSeqNum;
    }
}