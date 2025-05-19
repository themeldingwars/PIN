namespace GameServer.Enums;

// https://github.com/themeldingwars/Documentation/wiki/Messages-Control
public enum ControlPacketType : byte
{
    CloseConnection = 0,
    MatrixAck = 2,
    ReliableGSSAck = 3,
    TimeSyncRequest = 4,
    TimeSyncResponse = 5,
    MTUProbe = 6
}