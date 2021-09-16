using MyGameServer.Enums;
using Shared.Udp;

namespace MyGameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.WelcomeToTheMatrix)]
    public class WelcomeToTheMatrix
    {
        [Field] public ulong InstanceID;

        [Field] public ushort Unk1;

        [Field] public ushort Unk2;
    }
}