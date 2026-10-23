namespace GameServer.Protocol;

public enum ClientStatus
{
    Unknown = 0,
    Connecting = 1,
    Connected,
    Idle,
    Disconnecting,
    Aborted
}
