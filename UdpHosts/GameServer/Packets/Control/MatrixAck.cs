using GameServer.Enums;
using Shared.Udp;

namespace GameServer.Packets.Control;

[ControlMessage(ControlPacketType.MatrixAck)]
public class MatrixAck
{
    [Field]
    public ushort NextSeqNum;

    [Field]
    public ushort AckFor;
}