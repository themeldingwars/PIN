#nullable enable

namespace GameServer;

public enum ChannelType : byte
{
    Control = 0,
    Matrix = 1,
    ReliableGss = 2,
    UnreliableGss = 3
}