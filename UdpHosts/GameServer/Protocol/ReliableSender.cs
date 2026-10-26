#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Protocol;

/// <summary>
///     Numbers the frames of a reliable channel and keeps them until the client acks them. The client throws away
///     frames 256 or more past the oldest one it hasn't delivered yet, and since that leaves a gap it never gets
///     past, at most <see cref="Window" /> frames are on their way at any time.
/// </summary>
internal sealed class ReliableSender
{
    public const int Window = 256;

    private const int _sequenceOffset = GamePacketHeader.Size;

    private readonly ConcurrentQueue<byte[][]> _queued = new();
    private readonly Queue<byte[]> _waiting = new();
    private readonly List<InFlight> _inFlight = [];

    private ushort _nextSequence = 1;

    /// <summary>Frames waiting for room in the window.</summary>
    public int Waiting => _waiting.Count + _queued.Count;

    public int InFlightCount => _inFlight.Count;

    /// <summary>
    ///     Takes the frames of one message. The sequence number gets written when a frame goes out, so the fragments
    ///     of a split message stay consecutive even when several threads send at once.
    /// </summary>
    public void Enqueue(byte[][] frames)
    {
        _queued.Enqueue(frames);
    }

    /// <summary>
    ///     Sends what fits into the window. Has to run on the thread that handles the acks.
    /// </summary>
    public void Pump(DateTime now, Action<Memory<byte>> send)
    {
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
    }

    /// <summary>
    ///     Handles a MatrixAck or GSSAck: everything up to <paramref name="cumulative" /> arrived, and so did each of
    ///     the selective acks past a gap.
    /// </summary>
    /// <returns>The acked frames, oldest first</returns>
    public List<InFlight> Acked(ushort cumulative, ReadOnlySpan<ushort> selective)
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
            }
        }

        acked.Reverse();
        return acked;
    }

    internal sealed class InFlight(ushort sequence, byte[] frame, DateTime sentAt)
    {
        public ushort Sequence { get; } = sequence;
        public byte[] Frame { get; } = frame;
        public DateTime SentAt { get; set; } = sentAt;
        public int SendCount { get; set; } = 1;
    }
}
