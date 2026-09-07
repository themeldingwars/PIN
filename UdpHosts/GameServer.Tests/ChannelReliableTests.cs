using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aero.Protocol;
using GameServer.Packets;
using GameServer.Tests.Fakes;
using Serilog;
using Xunit;

namespace GameServer.Tests;

/// <summary>
///     Tests for the receiving half of the sequenced channels: what the server acknowledges, and what it does
///     with the fragments of a message that was split over several packets.
///
///     Both are on the path that produced the "Connection Problem" the client reported while standing on a boost
///     panel: the server only acknowledged packets with a sequence number above the last one it had acked, so a
///     retransmission (which the client sends because its ack got delayed) was never answered, and the split
///     reassembler neither acked its fragments nor survived a repeated one. A channel in that state stops
///     dispatching and stops acknowledging, while the server process itself keeps running, which is exactly what
///     the client reports.
/// </summary>
public class ChannelReliableTests
{
    private const ChannelType Reliable = ChannelType.ReliableGss;

    [Fact]
    public void ReliablePacket_IsAcknowledged()
    {
        var (channel, client, delivered) = CreateChannel(Reliable);

        channel.HandlePacket(Sequenced(Reliable, 0x0505));
        channel.Process(CancellationToken.None);

        Assert.Single(delivered);
        Assert.Equal(new[] { (ushort)0x0505 }, AckedSequenceNumbers(client));
    }

    [Fact]
    public void RetransmittedPacket_IsAcknowledgedAgain()
    {
        var (channel, client, _) = CreateChannel(Reliable);

        // The original, and the retransmission the client sent because it never saw the answer.
        channel.HandlePacket(Sequenced(Reliable, 0x0505));
        channel.HandlePacket(Sequenced(Reliable, 0x0505, resendCount: 1));
        channel.Process(CancellationToken.None);

        Assert.Equal(new[] { (ushort)0x0505, (ushort)0x0505 }, AckedSequenceNumbers(client));
        Assert.All(client.SentAcks, ack => Assert.Equal(Reliable, ack.Channel));
    }

    [Fact]
    public void SequenceNumberAfterWrap_IsAcknowledged()
    {
        var (channel, client, _) = CreateChannel(Reliable);

        channel.HandlePacket(Sequenced(Reliable, 0xFFFF));
        channel.HandlePacket(Sequenced(Reliable, 0x0101));
        channel.Process(CancellationToken.None);

        Assert.Contains((ushort)0x0101, AckedSequenceNumbers(client));
    }

    [Fact]
    public void UnreliableChannel_IsNotAcknowledged()
    {
        var (channel, client, delivered) = CreateChannel(ChannelType.UnreliableGss);

        channel.HandlePacket(Sequenced(ChannelType.UnreliableGss, 0x0707));
        channel.Process(CancellationToken.None);

        Assert.Single(delivered);
        Assert.Empty(client.SentAcks);
    }

    [Fact]
    public void SplitMessage_AcksEveryFragmentAndIsDispatchedOnce()
    {
        var (channel, client, delivered) = CreateChannel(Reliable);

        channel.HandlePacket(Sequenced(Reliable, 0x0101, isSplit: true, payload: [0xAA]));
        channel.HandlePacket(Sequenced(Reliable, 0x0202, isSplit: true, payload: [0xBB]));
        channel.HandlePacket(Sequenced(Reliable, 0x0303, payload: [0xCC]));
        channel.Process(CancellationToken.None);

        Assert.Equal(
            new[]
            {
                (ushort)0x0101,
                (ushort)0x0202,
                (ushort)0x0303,
            },
            AckedSequenceNumbers(client));

        var message = Assert.Single(delivered);
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC }, message.PacketData.Span.ToArray());
    }

    [Fact]
    public void RepeatedFragment_AssemblesWithoutThrowing()
    {
        var (channel, client, delivered) = CreateChannel(Reliable);

        channel.HandlePacket(Sequenced(Reliable, 0x0101, isSplit: true, payload: [0xAA]));

        // The retransmission of a fragment carries the same sequence number again. It used to be inserted into
        // the reassembler a second time, and SortedDictionary.Add threw over it, taking the shard thread down.
        channel.HandlePacket(Sequenced(Reliable, 0x0101, isSplit: true, resendCount: 1, payload: [0xAA]));
        channel.HandlePacket(Sequenced(Reliable, 0x0202, payload: [0xBB]));
        channel.Process(CancellationToken.None);

        var message = Assert.Single(delivered);
        Assert.Equal(new byte[] { 0xAA, 0xBB }, message.PacketData.Span.ToArray());
        Assert.Equal(3, client.SentAcks.Count);
    }

    [Fact]
    public void EndlessSplitMessage_IsGivenUpOnInsteadOfWedgingTheChannel()
    {
        var (channel, _, delivered) = CreateChannel(Reliable);

        // A message that keeps sending fragments and never terminates (corrupted length, a client that died
        // halfway) must not leave the channel in split mode forever: while it is, nothing else is dispatched.
        // One fragment more than the reassembler is willing to hold for a single message.
        for (ushort fragment = 1; fragment <= 129; fragment++)
        {
            channel.HandlePacket(Sequenced(Reliable, Palindrome(fragment), isSplit: true, payload: [0x01]));
        }

        channel.HandlePacket(Sequenced(Reliable, 0x4040, payload: [0x42]));
        channel.Process(CancellationToken.None);

        var message = Assert.Single(delivered);
        Assert.Equal(new byte[] { 0x42 }, message.PacketData.Span.ToArray());
    }

    private static ushort[] AckedSequenceNumbers(FakeNetworkPlayer client)
    {
        return [.. client.SentAcks.Select(ack => ack.SequenceNumber)];
    }

    /// <summary>
    ///     Sequence numbers that read the same whichever way round the bytes end up, so the tests do not depend
    ///     on the endianness the packet reader happens to use.
    /// </summary>
    private static ushort Palindrome(ushort value)
    {
        return (ushort)((value << 8) | value);
    }

    private static (Channel Channel, FakeNetworkPlayer Client, List<GamePacket> Delivered) CreateChannel(ChannelType type)
    {
        var client = new FakeNetworkPlayer();
        var logger = new LoggerConfiguration().CreateLogger();

        var channel = Channel.GetChannels(client, logger, GssVersion.V67, MatrixVersion.V26)[type];

        var delivered = new List<GamePacket>();
        channel.PacketAvailable += packet => delivered.Add(packet);

        return (channel, client, delivered);
    }

    private static GamePacket Sequenced(
        ChannelType type,
        ushort sequenceNumber,
        bool isSplit = false,
        byte resendCount = 0,
        byte[] payload = null)
    {
        payload ??= [0x2A];

        // The channel reads the sequence number and flips its bytes, so pre-flipping them here leaves the value
        // intact whichever byte order the reader uses.
        var swapped = Shared.Udp.Utils.SimpleFixEndianness(sequenceNumber);

        var data = new byte[2 + payload.Length];
        data[0] = (byte)(swapped >> 8);
        data[1] = (byte)(swapped & 0xFF);
        payload.CopyTo(data, 2);

        if (resendCount > 0)
        {
            // Retransmitted packets are scrambled so a resent packet can be told apart; the channel undoes this.
            var xor = new byte[] { 0xFF, 0xAA, 0xCC }[resendCount - 1];
            for (var i = 2; i < data.Length; i++)
            {
                data[i] ^= xor;
            }
        }

        return new GamePacket(new GamePacketHeader(type, resendCount, isSplit, (ushort)data.Length), data);
    }
}
