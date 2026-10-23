using System;
using System.Buffers.Binary;
using Shared.Udp;
namespace GameServer.Protocol;

public struct GamePacket
{
    /// <summary>
    ///     Size of the GSS message header prepended to every game shaped message
    /// </summary>
    public const int GssHeaderSize = 9;

    public readonly GamePacketHeader Header;

    /// <summary>
    ///     All of the data received from the client except the socket id and header bytes
    /// </summary>
    public readonly ReadOnlyMemory<byte> PacketData;

    public GamePacket(GamePacketHeader hdr, ReadOnlyMemory<byte> data, DateTime? received = null)
    {
        Header = hdr;
        PacketData = data;
        CurrentPosition = 0;
        Received = received ?? DateTime.Now;
    }

    /// <summary>
    ///     The position of the first unread byte within the packet data
    /// </summary>
    public int CurrentPosition { get; private set; }

    /// <summary>
    ///     The count of bytes in the packet, including the header
    /// </summary>
    public readonly int TotalBytes => PacketData.Length;

    /// <summary>
    ///     The count of bytes that haven't been read yet considering <see cref="CurrentPosition" /> and <see cref="TotalBytes" />
    /// </summary>
    public readonly int BytesRemaining => TotalBytes - CurrentPosition;

    /// <summary>
    ///     The time the packet was received by the server
    /// </summary>
    public DateTime Received { get; set; }

    public T Read<T>()
    {
        var buf = PacketData[CurrentPosition..];
        var ret = Deserializer.Read<T>(ref buf);

        CurrentPosition = TotalBytes - buf.Length;

        return ret;
    }

    public ReadOnlyMemory<byte> Read(int len)
    {
        var p = CurrentPosition;
        CurrentPosition += len;

        return PacketData.Slice(p, len);
    }

    public readonly T Peek<T>()
        where T : struct
    {
        var buf = PacketData[CurrentPosition..];
        return Deserializer.Read<T>(ref buf);
    }

    public readonly ReadOnlyMemory<byte> Peek(int len)
    {
        return PacketData.Slice(CurrentPosition, len);
    }

    public void Skip(int len)
    {
        CurrentPosition += len;
    }

    public void Reset()
    {
        CurrentPosition = 0;
    }

    /// <summary>
    ///     Read a single byte without going through the reflection based deserializer
    /// </summary>
    /// <returns>The byte that was read</returns>
    /// <exception cref="ArgumentOutOfRangeException">There are no bytes left to read</exception>
    public byte ReadByte()
    {
        if (BytesRemaining < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentPosition), "Packet underflow while reading a byte");
        }

        return PacketData.Span[CurrentPosition++];
    }

    /// <summary>
    ///     Read a big endian (network order) unsigned short if enough bytes remain
    /// </summary>
    /// <param name="value">The value that was read, 0 when the packet was too short</param>
    /// <returns>true if enough bytes remained to read the value, false otherwise</returns>
    public bool TryReadUInt16BigEndian(out ushort value)
    {
        if (BytesRemaining < 2)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadUInt16BigEndian(PacketData.Span[CurrentPosition..]);
        CurrentPosition += 2;
        return true;
    }

    /// <summary>
    ///     Read the 9 byte GSS message header: typecode, 7 byte entity id and message id
    /// </summary>
    /// <param name="typecode">Typecode of the entity the message is for</param>
    /// <param name="entityId">Id of the entity the message is for, with the typecode byte cleared</param>
    /// <param name="messageId">Message id relative to the typecode</param>
    /// <returns>true if enough bytes remained to read the header, false otherwise</returns>
    public bool TryReadGssHeader(out byte typecode, out ulong entityId, out byte messageId)
    {
        if (BytesRemaining < GssHeaderSize)
        {
            typecode = 0;
            entityId = 0;
            messageId = 0;
            return false;
        }

        var span = PacketData.Span[CurrentPosition..];
        typecode = span[0];
        entityId = BinaryPrimitives.ReadUInt64LittleEndian(span) & 0xffffffffffffff00;
        messageId = span[8];
        CurrentPosition += GssHeaderSize;
        return true;
    }
}
