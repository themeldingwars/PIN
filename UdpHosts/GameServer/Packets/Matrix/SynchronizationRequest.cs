using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.Matrix;

[MatrixMessage(MatrixPacketType.SynchronizationRequest)]
public class SynchronizationRequest
{
    [Field]
    [Length(1)]
    public IList<byte> Unk;
}