using Shared.Udp;
using System;

namespace GameServer.Packets;

public struct GamePacket
{
    public readonly GamePacketHeader Header;
    public readonly ReadOnlyMemory<byte> PacketData;
    public int CurrentPosition { get; private set; }
    public int TotalBytes => PacketData.Length;
    public int BytesRemaining => TotalBytes - CurrentPosition;
    public DateTime Recieved { get; set; }

    public GamePacket(GamePacketHeader hdr, ReadOnlyMemory<byte> data, DateTime? received = null)
    {
        Header = hdr;
        PacketData = data;
        CurrentPosition = 0;
        Recieved = received ?? DateTime.Now;
    }

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