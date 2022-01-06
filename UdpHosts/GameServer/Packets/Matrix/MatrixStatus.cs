using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Packets.Matrix;

[MatrixMessage(MatrixPacketType.MatrixStatus)]
public class MatrixStatus
{
    [Field]
    [Length(16)]
    public IList<byte> Unk;

    public MatrixStatus()
    {
        Unk = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }.ToList();
    }
}