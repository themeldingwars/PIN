using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace GameServer.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
public readonly struct GamePacketHeader
{
    /// <summary>
    ///     Size of the on-wire header, in bytes
    /// </summary>
    public const int Size = 2;

    public readonly ushort PacketHeader;

    public GamePacketHeader(ChannelType channel, byte resendCount, bool isSplit, ushort len)
    {
        PacketHeader = (ushort)((((byte)channel & 0b11) << 14) |
                                ((resendCount & 0b11) << 12) |
                                ((isSplit ? 1 : 0) << 11) |
                                (len & 0x07FF));
    }

    public GamePacketHeader(ushort rawHeader)
    {
        PacketHeader = rawHeader;
    }

    public ChannelType Channel => (ChannelType)(byte)(PacketHeader >> 14);
    public byte ResendCount => (byte)((PacketHeader >> 12) & 0b11);
    public bool IsSplit => ((PacketHeader >> 11) & 0b1) == 1;
    public ushort Length => (ushort)(PacketHeader & 0x7ff);

    /// <summary>
    ///     Read a header from network byte order
    /// </summary>
    /// <param name="source">Source, at least <see cref="Size" /> bytes long</param>
    /// <returns>The parsed header</returns>
    public static GamePacketHeader Read(ReadOnlySpan<byte> source)
    {
        return new GamePacketHeader(BinaryPrimitives.ReadUInt16BigEndian(source));
    }

    /// <summary>
    ///     Write the header to its destination in network byte order
    /// </summary>
    /// <param name="destination">Destination, at least <see cref="Size" /> bytes long</param>
    public void Write(Span<byte> destination)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, PacketHeader);
    }
}
