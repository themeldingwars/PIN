using GameServer.Enums;

namespace GameServer.Packets.Control;

[ControlMessage(ControlPacketType.CloseConnection)]
public class CloseConnection
{
    public uint Unknown1;
}