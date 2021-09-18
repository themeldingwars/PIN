using System.Runtime.InteropServices;

namespace GameServer.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public readonly struct GamePacketHeader
    {
        public readonly ushort PacketHeader;

        public ChannelType Channel => (ChannelType)(byte)(PacketHeader >> 14);
        public byte ResendCount => (byte)((PacketHeader >> 12) & 0b11);
        public bool IsSplit => ((PacketHeader >> 11) & 0b1) == 1;
        public ushort Length => (ushort)(PacketHeader & 0x7ff);

        public GamePacketHeader(ChannelType channel, byte resendCount, bool isSplit, ushort len)
        {
            PacketHeader = (ushort)((((byte)channel & 0b11) << 14) |
                                    ((resendCount & 0b11) << 12) |
                                    ((isSplit ? 1 : 0) << 11) |
                                    (len & 0x07FF));
        }
    }
}