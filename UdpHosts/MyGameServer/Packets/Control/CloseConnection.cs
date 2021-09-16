using MyGameServer.Enums;

namespace MyGameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.CloseConnection)]
    public class CloseConnection
    {
        public uint Unk1;
    }
}