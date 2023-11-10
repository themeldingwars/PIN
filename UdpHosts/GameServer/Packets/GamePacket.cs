using System;
using Shared.Udp;
namespace GameServer.Packets;

public struct GamePacket
{
    public readonly GamePacketHeader Header;

    /// <summary>
    ///     All of the data received from the client except the socket id
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
    public int TotalBytes => PacketData.Length;

    /// <summary>
    ///     The count of bytes that haven't been read yet considering <see cref="CurrentPosition" /> and <see cref="TotalBytes" />
    /// </summary>
    public int BytesRemaining => TotalBytes - CurrentPosition;

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

    public T Peek<T>()
        where T : struct
    {
        var buf = PacketData[CurrentPosition..];
        return Deserializer.Read<T>(ref buf);
    }

    public ReadOnlyMemory<byte> Peek(int len)
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
}