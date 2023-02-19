using GameServer.Enums;
using Shared.Udp;

namespace GameServer.Packets.Matrix;

[MatrixMessage(MatrixPacketType.WelcomeToTheMatrix)]
public class WelcomeToTheMatrix
{
    [Field]
    public ulong InstanceID;

    [Field]
    public ushort Unknown1;

    [Field]
    public ushort Unknown2;
}