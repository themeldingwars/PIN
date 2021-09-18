using System;
using System.Runtime.InteropServices;

namespace GameServer.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public readonly struct GamePacketHeader
    {
        public readonly UInt16 PacketHeader;

        public ChannelType Channel => (ChannelType)(byte)(PacketHeader >> 14);
        public byte ResendCount => (byte)((PacketHeader >> 12) & 0b11);
        public bool IsSplit => ((PacketHeader >> 11) & 0b1) == 1;
        public UInt16 Length => (UInt16)(PacketHeader & 0x7ff);

        public GamePacketHeader(ChannelType channel, byte resendCount, bool isSplit, UInt16 len)
        {
            PacketHeader = (UInt16)((((byte)channel & 0b11) << 14) |
                                    ((resendCount & 0b11) << 12) |
                                    ((isSplit ? 1 : 0) << 11) |
                                    (len & 0x07FF));
        }
    }
}