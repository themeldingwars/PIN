﻿#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Aero.Gen;
using Aero.Gen.Attributes;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using Shared.Udp;

namespace GameServer;

public class Channel
{
    private const int ProtocolHeaderSize = 80; // UDP + IP
    private const int GameSocketHeaderSize = 4;
    private const int TotalHeaderSize = ProtocolHeaderSize + GameSocketHeaderSize;
    private const int MaxPacketSize = PacketServer.MTU - TotalHeaderSize;
    
    private static readonly byte[] XorByte = { 0x00, 0xFF, 0xCC, 0xAA };
    private static readonly ulong[] XorULong = { 0x00, 0xFFFFFFFFFFFFFFFF, 0xCCCCCCCCCCCCCCCC, 0xAAAAAAAAAAAAAAAA };

    private readonly ILogger _logger;

    private readonly INetworkClient _client;
    private readonly ConcurrentQueue<GamePacket> _incomingPackets;
    private readonly ConcurrentQueue<Memory<byte>> _outgoingPackets;

    private Channel(ChannelType channelType, bool isSequenced, bool isReliable, INetworkClient networkClient, ILogger logger)
    {
        Type = channelType;
        IsSequenced = isSequenced;
        IsReliable = isReliable;
        _client = networkClient;
        _logger = logger;
        CurrentSequenceNumber = 1;
        LastAck = 0;

        _incomingPackets = new ConcurrentQueue<GamePacket>();
        _outgoingPackets = new ConcurrentQueue<Memory<byte>>();
    }

    public delegate void PacketAvailableDelegate(GamePacket packet);
    
    public event PacketAvailableDelegate? PacketAvailable;
    
    private ChannelType Type { get; }
    private bool IsSequenced { get; }
    private bool IsReliable { get; }
    private ushort CurrentSequenceNumber { get; set; }
    private DateTime LastActivity { get; set; }
    private ushort LastAck { get; set; }

    public static Dictionary<ChannelType, Channel> GetChannels(INetworkClient client, ILogger logger)
    {
        return new Dictionary<ChannelType, Channel>
               {
                   { ChannelType.Control, new Channel(ChannelType.Control, false, false, client, logger) },
                   { ChannelType.Matrix, new Channel(ChannelType.Matrix, true, true, client, logger) },
                   { ChannelType.ReliableGss, new Channel(ChannelType.ReliableGss, true, true, client, logger) },
                   { ChannelType.UnreliableGss, new Channel(ChannelType.UnreliableGss, true, false, client, logger) }
               };
    }

    public void HandlePacket(GamePacket packet)
    {
        _incomingPackets.Enqueue(packet);
    }

    public void Process(CancellationToken ct)
    {
        while (_outgoingPackets.TryDequeue(out var qi))
        {
            _client.Send(qi);
            LastActivity = DateTime.Now;
        }

        while (_incomingPackets.TryDequeue(out var packet))
        {
            // Console.Write("> " + string.Concat(BitConverter.GetBytes(packet.Header.PacketHeader).ToArray().Select(b => b.ToString("X2")).ToArray()));
            // Console.WriteLine(" "+string.Concat(packet.PacketData.ToArray().Select(b => b.ToString("X2")).ToArray()));
            ushort sequenceNumber = 0;
            if (IsSequenced)
            {
                sequenceNumber = Utils.SimpleFixEndianness(packet.Read<ushort>());
            }
            
            // TODO: Implement SequencedPacketQueue
            if (packet.Header.ResendCount > 0)
            {
                // de-xor data
                var x = packet.PacketData.Length >> 3;
                var data = packet.PacketData.ToArray();
                var dataTemp = Array.Empty<byte>();

                if (x > 0)
                {
                    var uSpan = MemoryMarshal.Cast<byte, ulong>(data);

                    for (var i = 0; i < x; i++)
                    {
                        uSpan[i] ^= XorULong[packet.Header.ResendCount];
                    }

                    dataTemp = MemoryMarshal.Cast<ulong, byte>(uSpan).ToArray(); // override old size
                }

                // copy back
                for (var i = 0; i < dataTemp.Length; i++)
                {
                    data[i] = dataTemp[i];
                }

                for (var i = x * 8; i < packet.PacketData.Length; i++)
                {
                    data[i] ^= XorByte[packet.Header.ResendCount];
                }

                // for( int i = 0; i < data.Length; i++ )
                //     data[i] ^= xorByte[packet.Header.ResendCount];
                packet = new GamePacket(packet.Header, new ReadOnlyMemory<byte>(data));
                _logger.Fatal("---> Resent packet!!! C:{0}: {1} bytes", Type, packet.TotalBytes);
            }

            if (packet.Header.IsSplit)
            {
                _logger.Fatal("---> Split packet!!! C:{0}: {1} bytes", Type, packet.TotalBytes);
            }

            if (IsReliable && (sequenceNumber > LastAck || (sequenceNumber < 0xff && LastAck > 0xff00)))
            {
                _client.SendAck(Type, sequenceNumber, packet.Received);
                LastAck = sequenceNumber;
            }

            PacketAvailable?.Invoke(packet);
            LastActivity = DateTime.Now;
        }

        if ((DateTime.Now - LastActivity).TotalMilliseconds > 100)
        {
            // Send heartbeat?
        }
    }

    public bool Send<T>(T packet)
        where T : struct
    {
        Memory<byte> packetToSend;
        if (packet is IWritable write)
        {
            packetToSend = write.Write();
        }
        else
        {
            packetToSend = Serializer.WriteStruct(packet);
        }

        if (typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault() is ControlMessageAttribute controlMessageAttribute)
        {
            var messageId = controlMessageAttribute.MsgID;
            var t = new Memory<byte>(new byte[1 + packetToSend.Length]);
            packetToSend.CopyTo(t[1..]);
            Serializer.WritePrimitive((byte)messageId).CopyTo(t);
            packetToSend = t;
            _logger.Verbose("<-- {0}: MsgID = {1} ({2})", Type, messageId, (byte)messageId);
        }
        else if (typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault() is MatrixMessageAttribute matrixMessageAttribute)
        {
            var messageId = matrixMessageAttribute.MsgID;
            var t = new Memory<byte>(new byte[1 + packetToSend.Length]);
            packetToSend.CopyTo(t[1..]);
            Serializer.WritePrimitive((byte)messageId).CopyTo(t);
            packetToSend = t;
            _logger.Verbose("<-- {0}: MsgID = {1} ({2})", Type, messageId, (byte)messageId);
        }
        else
        {
            throw new Exception();
        }

        return Send(packetToSend);
    }

    public bool SendClass<T>(T packet, Type? messageEnumType = null)
        where T : class
    {
        Memory<byte> packetToSend;
        if (packet is IWritable write)
        {
            packetToSend = write.Write();
        }
        else
        {
            packetToSend = Serializer.WriteClass(packet);
        }

        byte messageId;

        if (typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault() is ControlMessageAttribute controlMessageAttribute)
        {
            messageId = (byte)controlMessageAttribute.MsgID;
        }
        else if (typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault() is MatrixMessageAttribute matrixMessageAttribute)
        {
            messageId = (byte)matrixMessageAttribute.MsgID;
        }
        else
        {
            throw new Exception();
        }

        var t = new Memory<byte>(new byte[1 + packetToSend.Length]);
        packetToSend.CopyTo(t[1..]);

        Serializer.WritePrimitive(messageId).CopyTo(t);

        packetToSend = t;

        if (messageEnumType == null)
        {
            _logger.Verbose("<-- {0}: MsgID = 0x{1:X2}", Type, messageId);
        }
        else
        {
            _logger.Verbose("<-- {0}: MsgID = {1} (0x{2:X2})", Type, Enum.Parse(messageEnumType, Enum.GetName(messageEnumType, messageId) ?? string.Empty), messageId);
        }

        return Send(packetToSend);
    }

    public bool SendGSS<T>(T packet, ulong entityId, Enums.GSS.Controllers? controllerId = null, Type? messageEnumType = null)
        where T : struct
    {
        Memory<byte> packetToSend;
        if (packet is IWritable write)
        {
            packetToSend = write.Write();
        }
        else
        {
            packetToSend = Serializer.WriteStruct(packet);
        }

        if (typeof(T).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() is GSSMessageAttribute gssMessageAttribute)
        {
            var messageId = gssMessageAttribute.MsgID;
            var t = new Memory<byte>(new byte[9 + packetToSend.Length]);
            packetToSend.CopyTo(t[9..]);

            Serializer.WritePrimitive(entityId).CopyTo(t);

            // Intentionally overwrite first byte of Entity ID
            if (controllerId.HasValue)
            {
                Serializer.WritePrimitive((byte)controllerId.Value).CopyTo(t);
            }
            else if (gssMessageAttribute.ControllerID.HasValue)
            {
                Serializer.WritePrimitive((byte)gssMessageAttribute.ControllerID.Value).CopyTo(t);
            }
            else
            {
                throw new Exception();
            }

            Serializer.WritePrimitive(messageId).CopyTo(t[8..]);

            packetToSend = t;

            if (messageEnumType == null)
            {
                _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}", Type, controllerId ?? gssMessageAttribute.ControllerID, entityId, messageId);
            }
            else
            {
                _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})",
                                Type,
                                controllerId ?? gssMessageAttribute.ControllerID,
                                entityId,
                                Enum.Parse(messageEnumType, Enum.GetName(messageEnumType, messageId) ?? string.Empty),
                                messageId);
            }
        }
        else
        {
            throw new Exception();
        }

        return Send(packetToSend);
    }

    /// <summary>
    ///     Send a GSS class to the client
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet</typeparam>
    /// <param name="packet"></param>
    /// <param name="entityId"></param>
    /// <param name="controllerIdParameter">If not provided on the <see cref="GSSMessageAttribute" /> on the packet, the controller Id may be specified here</param>
    /// <param name="messageEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if either of the following applies:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><see cref="TPacket" /> is not annotated with <see cref="GSSMessageAttribute" /></description>
    ///         </item>
    ///         <item>
    ///             <description>The <see cref="GSSMessageAttribute.ControllerID" /> value on the <see cref="GSSMessageAttribute" /> of <see cref="TPacket" /> and <see cref="controllerIdParameter" /> both are null</description>
    ///         </item>
    ///     </list>
    /// </exception>
    public bool SendGSSClass<TPacket>(TPacket packet, ulong entityId, Enums.GSS.Controllers? controllerIdParameter = null, Type? messageEnumType = null)
        where TPacket : class
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() is not GSSMessageAttribute gssMessageAttribute)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(GSSMessageAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var messageId = gssMessageAttribute.MsgID;

        var controllerId =
            controllerIdParameter ??
            gssMessageAttribute.ControllerID ??
            throw new ArgumentException($"No controller Id was provided, neither from the {nameof(GSSMessageAttribute)} present on {typeof(TPacket).FullName} nor via parameter");

        Memory<byte> packetToSend;
        if (packet is IWritable write)
        {
            packetToSend = write.Write();
        }
        else
        {
            packetToSend = Serializer.WriteClass(packet);
        }

        // Console.WriteLine( string.Concat( packetData.ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) );
        return SendPacketMemory(entityId, messageId, controllerId, ref packetToSend, messageEnumType);
    }

    /// <summary>
    ///     Send an <see cref="IAero" /> package to the client
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet</typeparam>
    /// <param name="packet"></param>
    /// <param name="entityId"></param>
    /// <param name="messageIdOverride">Optional way to override the message ID</param>
    /// <param name="messageEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException"></exception>
    public bool SendIAero<TPacket>(TPacket packet, ulong entityId = 0, byte messageIdOverride = 0, Type? messageEnumType = null)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMessageIdAttribute)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var type = aeroMessageIdAttribute.Typ;
        var messageId = (byte)aeroMessageIdAttribute.MessageId;
        if (messageIdOverride != 0)
        {
            messageId = messageIdOverride;
        }

        packet.SerializeToMemory(out var packetMemory);

        switch (type)
        {
            case AeroMessageIdAttribute.MsgType.Matrix:
                return SendPacketMemoryMatrix(messageId, ref packetMemory, messageEnumType);
            case AeroMessageIdAttribute.MsgType.GSS:
                {
                    var controllerId = (Enums.GSS.Controllers)aeroMessageIdAttribute.ControllerId;
                    
                    // Program.Logger.Information($"SendIAero type {type}, controller {controllerId} and msgid {messageId}");
                    return SendPacketMemory(entityId, messageId, controllerId, ref packetMemory, messageEnumType);
                }
            
            case AeroMessageIdAttribute.MsgType.Control:
            default:
                throw new ArgumentException("Message type not implemented");
        }
    }

    public bool SendIAeroControllerKeyframe<TPacket>(TPacket packet, ulong entityId, ulong playerId)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var controllerId = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;
        packet.SerializeToMemory(out var packetMemory);
        var messageData = new Memory<byte>(new byte[8 + packetMemory.Length]);
        packetMemory.CopyTo(messageData[8..]);
        Serializer.WritePrimitive(playerId).CopyTo(messageData);
        return SendPacketMemory(entityId, 4, controllerId, ref messageData);
    }

    public bool SendIAeroChanges<TPacket>(TPacket packet, ulong entityId)
        where TPacket : class, IAeroViewInterface
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var typeCode = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;

        packet.SerializeChangesToMemory(out var packetMemory);

        return SendPacketMemory(entityId, 1, typeCode, ref packetMemory);
    }

    /// <summary>
    ///     Send serialized data of a gss channel packet to the client
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="messageId"></param>
    /// <param name="controllerId"></param>
    /// <param name="packetToSend"></param>
    /// <param name="msgEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="InvalidOperationException">If <see cref="msgEnumType" /> is not null and does not contain an element with a value equal to <see cref="messageId" /> </exception>
    private bool SendPacketMemory(ulong entityId,
                                  byte messageId,
                                  Enums.GSS.Controllers controllerId,
                                  ref Memory<byte> packetToSend,
                                  Type? msgEnumType = null)
    {
        const int HeaderByteSize = 9;

        var serializedData = new Memory<byte>(new byte[HeaderByteSize + packetToSend.Length]);
        packetToSend.CopyTo(serializedData[HeaderByteSize..]);

        Serializer.WritePrimitive(entityId).CopyTo(serializedData);

        // Intentionally overwrite first byte of Entity ID
        Serializer.WritePrimitive((byte)controllerId).CopyTo(serializedData);
        Serializer.WritePrimitive(messageId).CopyTo(serializedData[8..]);

        if (msgEnumType == null)
        {
            _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}",
                            Type,
                            controllerId,
                            entityId,
                            messageId);
        }
        else
        {
            _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})",
                            Type,
                            controllerId,
                            entityId,
                            Enum.Parse(msgEnumType,
                                       Enum.GetName(msgEnumType, messageId) ?? throw new InvalidOperationException($"Message enum type {msgEnumType.FullName} has no element with a value of {messageId}")),
                            messageId);
        }

        // Program.Logger.Verbose( "<--- Sending {0} bytes", packetData.Length );
        return Send(serializedData);
    }

    /// <summary>
    ///     Send serialized data of a matrix channel packet to the client
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="packetMemory"></param>
    /// <param name="msgEnumType">TODO: Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    private bool SendPacketMemoryMatrix(byte messageId, ref Memory<byte> packetMemory, Type? msgEnumType = null)
    {
        const int HeaderByteSize = 1;
        var serializedData = new Memory<byte>(new byte[HeaderByteSize + packetMemory.Length]);
        packetMemory.CopyTo(serializedData[HeaderByteSize..]);
        Serializer.WritePrimitive(messageId).CopyTo(serializedData);
        return Send(serializedData);
    }

    /// <summary>
    ///     Send data to the client
    /// </summary>
    /// <param name="packetData"></param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    private bool Send(Memory<byte> packetData)
    {
        var headerLength = 2;
        if (IsSequenced)
        {
            headerLength += 2;
        }

        // TODO: Send UGSS messages that are split over RGSS
        while (packetData.Length > 0)
        {
            var length = Math.Min(packetData.Length + headerLength, MaxPacketSize);

            var t = new Memory<byte>(new byte[length]);
            packetData[.. (length - headerLength)].CopyTo(t[headerLength..]);

            if (IsSequenced)
            {
                if (IsReliable)
                {
                    _logger.Verbose("<- {0} SeqNum =  {1}", Type, CurrentSequenceNumber);
                }

                Serializer.WritePrimitive(Utils.SimpleFixEndianness(CurrentSequenceNumber)).CopyTo(t.Slice(2, 2));
                unchecked
                {
                    CurrentSequenceNumber++;
                }
            }

            var header = new GamePacketHeader(Type, 0, packetData.Length + headerLength > MaxPacketSize, (ushort)t.Length);
            var headerData = Serializer.WritePrimitive(Utils.SimpleFixEndianness(header.PacketHeader));
            headerData.CopyTo(t);

            // Console.Write("< "+string.Concat(BitConverter.GetBytes(Utils.SimpleFixEndianness(hdr.PacketHeader)).ToArray().Select(b => b.ToString("X2")).ToArray()));
            // Console.WriteLine( " "+string.Concat( t.Slice(2).ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) )
            _outgoingPackets.Enqueue(t);

            packetData = packetData[(length - headerLength) ..];
        }

        return true;
    }

    protected struct QueueItem
    {
        public Packet Packet;
        public GamePacketHeader Header;
    }
}