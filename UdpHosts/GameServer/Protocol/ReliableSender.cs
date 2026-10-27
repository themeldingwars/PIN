#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Protocol;

/// <summary>
///     Numbers the frames of a reliable channel and keeps them until the client acks them. The client throws away
///     frames 256 or more past the oldest one it hasn't delivered yet, and since that leaves a gap it never gets
///     past, at most <see cref="Window" /> frames are on their way at any time. Frames that aren't acked in time get
///     sent again, with the same timing the 1962 client uses for its own reliable channels.
/// </summary>
internal sealed class ReliableSender
{
    public const int Window = 256;
    public const int MaxSends = 16;

    public static readonly TimeSpan InitialResendTimeout = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan MinResendTimeout = TimeSpan.FromMilliseconds(250);
    public static readonly TimeSpan MaxResendTimeout = TimeSpan.FromMilliseconds(2500);
    public static readonly TimeSpan FillTimeout = TimeSpan.FromSeconds(15);

    private const int _sequenceOffset = GamePacketHeader.Size;
    private const int _payloadOffset = _sequenceOffset + 2;

    private static readonly byte[] _resendXor = [0xFF, 0xAA, 0xCC];

    private readonly ConcurrentQueue<byte[][]> _queued = new();
    private readonly Queue<byte[]> _waiting = new();
    private readonly List<InFlight> _inFlight = [];

    private ushort _nextSequence = 1;
    private DateTime? _windowFullSince;

    /// <summary>Frames waiting for room in the window.</summary>
    public int Waiting => _waiting.Count + _queued.Count;

    public int InFlightCount => _inFlight.Count;

    public TimeSpan ResendTimeout { get; private set; } = InitialResendTimeout;

    public long Resent { get; private set; }

    /// <summary>
    ///     Takes the frames of one message. The sequence number gets written when a frame goes out, so the fragments
    ///     of a split message stay consecutive even when several threads send at once.
    /// </summary>
    public void Enqueue(byte[][] frames)
    {
        _queued.Enqueue(frames);
    }

    /// <summary>
    ///     Resends what wasn't acked in time and sends what fits into the window. Has to run on the thread that
    ///     handles the acks.
    /// </summary>
    /// <returns>Why the connection should be closed, the way the client words it, or null</returns>
    public string? Pump(DateTime now, Action<Memory<byte>> send)
    {
        foreach (var frame in _inFlight)
        {
            if (now - frame.SentAt < ResendTimeout)
            {
                continue;
            }

            if (frame.SendCount >= MaxSends)
            {
                return "excess packet loss";
            }

            ResendTimeout = ClampTimeout(ResendTimeout * 1.5);
            frame.SendCount++;
            frame.SentAt = now;
            Resent++;
            send(ResendCopy(frame));
        }

        while (_queued.TryDequeue(out var message))
        {
            foreach (var frame in message)
            {
                _waiting.Enqueue(frame);
            }
        }

        while (_waiting.Count > 0 && _inFlight.Count < Window)
        {
            var frame = _waiting.Dequeue();
            var sequence = _nextSequence++;
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(_sequenceOffset), sequence);
            _inFlight.Add(new InFlight(sequence, frame, now));
            send(frame);
        }

        if (_inFlight.Count < Window)
        {
            _windowFullSince = null;
            return null;
        }

        _windowFullSince ??= now;
        return now - _windowFullSince > FillTimeout ? "fill timeout" : null;
    }

    /// <summary>
    ///     Handles a MatrixAck or GSSAck: everything up to <paramref name="cumulative" /> arrived, and so did each of
    ///     the selective acks past a gap.
    /// </summary>
    /// <returns>The acked frames, oldest first</returns>
    public List<InFlight> Acked(ushort cumulative, ReadOnlySpan<ushort> selective, DateTime now)
    {
        var acked = new List<InFlight>();

        // An ack for a frame that never went out is garbage, it would clear frames the client hasn't seen
        var lastSent = (ushort)(_nextSequence - 1);
        if (unchecked((short)(cumulative - lastSent)) > 0)
        {
            return acked;
        }

        for (var i = _inFlight.Count - 1; i >= 0; i--)
        {
            var frame = _inFlight[i];
            if (unchecked((short)(cumulative - frame.Sequence)) >= 0 || selective.Contains(frame.Sequence))
            {
                acked.Add(frame);
                _inFlight.RemoveAt(i);

                // A resent frame's round trip could belong to any of its sends
                if (frame.SendCount == 1)
                {
                    var rtt = now - frame.SentAt;
                    ResendTimeout = ClampTimeout(((rtt / 2) + (rtt * 2) + ResendTimeout) / 2);
                }
            }
        }

        acked.Reverse();
        return acked;
    }

    private static TimeSpan ClampTimeout(TimeSpan timeout)
    {
        return timeout < MinResendTimeout ? MinResendTimeout : timeout > MaxResendTimeout ? MaxResendTimeout : timeout;
    }

    private static byte[] ResendCopy(InFlight frame)
    {
        var copy = (byte[])frame.Frame.Clone();
        var resendCount = (byte)((frame.SendCount % 3) + 1);
        var header = GamePacketHeader.Read(copy);
        new GamePacketHeader(header.Channel, resendCount, header.IsSplit, header.Length).Write(copy);

        var xor = _resendXor[resendCount - 1];
        for (var i = _payloadOffset; i < copy.Length; i++)
        {
            copy[i] ^= xor;
        }

        return copy;
    }

    internal sealed class InFlight(ushort sequence, byte[] frame, DateTime sentAt)
    {
        public ushort Sequence { get; } = sequence;
        public byte[] Frame { get; } = frame;
        public DateTime SentAt { get; set; } = sentAt;
        public int SendCount { get; set; } = 1;
    }
}
