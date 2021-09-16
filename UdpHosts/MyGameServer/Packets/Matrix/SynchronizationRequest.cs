using MyGameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace MyGameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.SynchronizationRequest)]
    public class SynchronizationRequest
    {
        [Field] [Length(1)] public IList<byte> Unk;
    }
}