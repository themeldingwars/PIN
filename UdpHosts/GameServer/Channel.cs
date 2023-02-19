#nullable enable
using Aero.Gen;
using Aero.Gen.Attributes;
using GameServer.Extensions;
using GameServer.Packets;
using Shared.Udp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameServer;

public enum ChannelType : byte
{
    Control = 0,
    Matrix = 1,
    ReliableGss = 2,
    UnreliableGss = 3
}

public class Channel
{
    public delegate void PacketAvailableDelegate(GamePacket packet);

    private const int ProtocolHeaderSize = 80; // UDP + IP
    private const int GameSocketHeaderSize = 4;
    private const int TotalHeaderSize = ProtocolHeaderSize + GameSocketHeaderSize;
    private const int MaxPacketSize = PacketServer.MTU - TotalHeaderSize;
    private static readonly byte[] xorByte = { 0x00, 0xFF, 0xCC, 0xAA };
    private static readonly ulong[] xorULong = { 0x00, 0xFFFFFFFFFFFFFFFF, 0xCCCCCCCCCCCCCCCC, 0xAAAAAAAAAAAAAAAA };

    protected INetworkClient client;
    protected ConcurrentQueue<GamePacket> incomingPackets;
    protected ConcurrentQueue<Memory<byte>> outgoingPackets;

    protected Channel(ChannelType ct, bool isSequenced, bool isReliable, INetworkClient c)
    {
        Type = ct;
        IsSequenced = isSequenced;
        IsReliable = isReliable;
        client = c;
        CurrentSequenceNumber = 1;
        LastAck = 0;

        incomingPackets = new ConcurrentQueue<GamePacket>();
        outgoingPackets = new ConcurrentQueue<Memory<byte>>();
    }

    public ChannelType Type { get; protected set; }
    public bool IsSequenced { get; protected set; }
    public bool IsReliable { get; protected set; }
    public ushort CurrentSequenceNumber { get; protected set; }
    public DateTime LastActivity { get; protected set; }
    public ushort LastAck { get; protected set; }

    public static Dictionary<ChannelType, Channel> GetChannels(INetworkClient client)
    {
        return new Dictionary<ChannelType, Channel>
               {
                   { ChannelType.Control, new Channel(ChannelType.Control, false, false, client) },
                   { ChannelType.Matrix, new Channel(ChannelType.Matrix, true, true, client) },
                   { ChannelType.ReliableGss, new Channel(ChannelType.ReliableGss, true, true, client) },
                   { ChannelType.UnreliableGss, new Channel(ChannelType.UnreliableGss, true, false, client) }
               };
    }

    public event PacketAvailableDelegate PacketAvailable;

    public void HandlePacket(GamePacket packet)
    {
        incomingPackets.Enqueue(packet);
    }

    public void Process(CancellationToken ct)
    {
        while (outgoingPackets.TryDequeue(out var qi))
        {
            client.Send(qi);
            LastActivity = DateTime.Now;
        }

        while (incomingPackets.TryDequeue(out var packet))
        {
            //Console.Write("> " + string.Concat(BitConverter.GetBytes(packet.Header.PacketHeader).ToArray().Select(b => b.ToString("X2")).ToArray()));
            //Console.WriteLine(" "+string.Concat(packet.PacketData.ToArray().Select(b => b.ToString("X2")).ToArray()));
            ushort seqNum = 0;
            if (IsSequenced)
            {
                seqNum = Utils.SimpleFixEndianness(packet.Read<ushort>());
            }
            // TODO: Implement SequencedPacketQueue

            if (packet.Header.ResendCount > 0)
            {
                // de-xor data
                var x = packet.PacketData.Length >> 3;
                var data = packet.PacketData.ToArray();
                var dataTmp = Array.Empty<byte>();

                if (x > 0)
                {
                    var uSpan = MemoryMarshal.Cast<byte, ulong>(data);

                    for (var i = 0; i < x; i++)
                    {
                        uSpan[i] ^= xorULong[packet.Header.ResendCount];
                    }

                    dataTmp = MemoryMarshal.Cast<ulong, byte>(uSpan).ToArray(); // override old size
                }

                for (var i = 0; i < dataTmp.Length; i++) // copy back
                {
                    data[i] = dataTmp[i];
                }

                for (var i = x * 8; i < packet.PacketData.Length; i++)
                {
                    data[i] ^= xorByte[packet.Header.ResendCount];
                }

                //for( int i = 0; i < data.Length; i++ )
                //	data[i] ^= xorByte[packet.Header.ResendCount];

                packet = new GamePacket(packet.Header, new ReadOnlyMemory<byte>(data));
                Program.Logger.Fatal("---> Resent packet!!! C:{0}: {1} bytes", Type, packet.TotalBytes);
            }

            if (packet.Header.IsSplit)
            {
                Program.Logger.Fatal("---> Split packet!!! C:{0}: {1} bytes", Type, packet.TotalBytes);
            }

            if (IsReliable && (seqNum > LastAck || (seqNum < 0xff && LastAck > 0xff00)))
            {
                client.SendAck(Type, seqNum, packet.Received);
                LastAck = seqNum;
            }

            PacketAvailable?.Invoke(packet);
            LastActivity = DateTime.Now;
        }

        if ((DateTime.Now - LastActivity).TotalMilliseconds > 100)
        {
            // Send heartbeat?
        }
    }

    public bool Send<T>(T pkt)
        where T : struct
    {
        Memory<byte> p;
        if (pkt is IWritable write)
        {
            p = write.Write();
        }
        else
        {
            p = Serializer.WriteStruct(pkt);
        }

        var conMsgAttr = typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault() as ControlMessageAttribute;
        var matMsgAttr = typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault() as MatrixMessageAttribute;

        if (conMsgAttr != null)
        {
            var msgID = conMsgAttr.MsgID;
            var t = new Memory<byte>(new byte[1 + p.Length]);
            p.CopyTo(t[1..]);
            Serializer.WritePrimitive((byte)msgID).CopyTo(t);
            p = t;
            Program.Logger.Verbose("<-- {0}: MsgID = {1} ({2})", Type, msgID, (byte)msgID);
        }
        else if (matMsgAttr != null)
        {
            var msgID = matMsgAttr.MsgID;
            var t = new Memory<byte>(new byte[1 + p.Length]);
            p.CopyTo(t[1..]);
            Serializer.WritePrimitive((byte)msgID).CopyTo(t);
            p = t;
            Program.Logger.Verbose("<-- {0}: MsgID = {1} ({2})", Type, msgID, (byte)msgID);
        }
        else
        {
            throw new Exception();
        }

        return Send(p);
    }

    public bool SendClass<T>(T pkt, Type msgEnumType = null)
        where T : class
    {
        Memory<byte> p;
        if (pkt is IWritable write)
        {
            p = write.Write();
        }
        else
        {
            p = Serializer.WriteClass(pkt);
        }

        byte messageId;

        if (typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault() is ControlMessageAttribute controlMsgAttr)
        {
            messageId = (byte)controlMsgAttr.MsgID;
        }
        else if (typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault() is MatrixMessageAttribute matrixMsgAttr)
        {
            messageId = (byte)matrixMsgAttr.MsgID;
        }
        else
        {
            throw new Exception();
        }

        var t = new Memory<byte>(new byte[1 + p.Length]);
        p.CopyTo(t[1..]);

        Serializer.WritePrimitive(messageId).CopyTo(t);

        p = t;

        if (msgEnumType == null)
        {
            Program.Logger.Verbose("<-- {0}: MsgID = 0x{1:X2}", Type, messageId);
        }
        else
        {
            Program.Logger.Verbose("<-- {0}: MsgID = {1} (0x{2:X2})", Type, Enum.Parse(msgEnumType, Enum.GetName(msgEnumType, messageId)), messageId);
        }

        return Send(p);
    }

    public bool SendGSS<T>(T pkt, ulong entityId, Enums.GSS.Controllers? controllerId = null, Type msgEnumType = null)
        where T : struct
    {
        Memory<byte> p;
        if (pkt is IWritable write)
        {
            p = write.Write();
        }
        else
        {
            p = Serializer.WriteStruct(pkt);
        }

        if (typeof(T).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() is GSSMessageAttribute gssMsgAttr)
        {
            var messageId = gssMsgAttr.MsgID;
            var t = new Memory<byte>(new byte[9 + p.Length]);
            p.CopyTo(t[9..]);

            Serializer.WritePrimitive(entityId).CopyTo(t);

            // Intentionally overwrite first byte of Entity ID
            if (controllerId.HasValue)
            {
                Serializer.WritePrimitive((byte)controllerId.Value).CopyTo(t);
            }
            else if (gssMsgAttr.ControllerID.HasValue)
            {
                Serializer.WritePrimitive((byte)gssMsgAttr.ControllerID.Value).CopyTo(t);
            }
            else
            {
                throw new Exception();
            }

            Serializer.WritePrimitive(messageId).CopyTo(t[8..]);

            p = t;

            if (msgEnumType == null)
            {
                Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}", Type, controllerId ?? gssMsgAttr.ControllerID.Value, entityId, messageId);
            }
            else
            {
                Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})", Type, controllerId ?? gssMsgAttr.ControllerID.Value, entityId,
                                       Enum.Parse(msgEnumType, Enum.GetName(msgEnumType, messageId)), messageId);
            }
        }
        else
        {
            throw new Exception();
        }

        return Send(p);
    }

    /// <summary>
    ///     Send a GSS class to the client
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet</typeparam>
    /// <param name="packet"></param>
    /// <param name="entityId"></param>
    /// <param name="controllerIdParameter">If not provided on the <see cref="GSSMessageAttribute" /> on the packet, the controller Id may be specified here</param>
    /// <param name="msgEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
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
    public bool SendGSSClass<TPacket>(TPacket packet, ulong entityId, Enums.GSS.Controllers? controllerIdParameter = null, Type? msgEnumType = null)
        where TPacket : class
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() is not GSSMessageAttribute gssMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(GSSMessageAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var messageId = gssMsgAttr.MsgID;


        var controllerId =
            controllerIdParameter ??
            gssMsgAttr.ControllerID ??
            throw new ArgumentException(
                                        $"No controller Id was provided, neither from the {nameof(GSSMessageAttribute)} present on {typeof(TPacket).FullName} nor via parameter"
                                       );

        Memory<byte> packetMemory;
        if (packet is IWritable write)
        {
            packetMemory = write.Write();
        }
        else
        {
            packetMemory = Serializer.WriteClass(packet);
        }

        //Console.WriteLine( string.Concat( packetData.ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) );

        return SendPacketMemory(entityId, messageId, controllerId, ref packetMemory, msgEnumType);
    }

    /// <summary>
    ///     Send an <see cref="IAero" /> package to the client
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet</typeparam>
    /// <param name="packet"></param>
    /// <param name="entityId"></param>
    /// <param name="msgEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException"></exception>
    public bool SendIAero<TPacket>(TPacket packet, ulong entityId, Type? msgEnumType = null) where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var controllerId = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;
        var messageId = (byte)aeroMsgAttr.MessageId;

        packet.SerializeToMemory(out var packetMemory);

        return SendPacketMemory(entityId, messageId, controllerId, ref packetMemory, msgEnumType);
    }


    /// <summary>
    ///     Send serialized data of a packet to the client
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="messageId"></param>
    /// <param name="controllerId"></param>
    /// <param name="packetMemory"></param>
    /// <param name="msgEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="InvalidOperationException">If <see cref="msgEnumType" /> is not null and does not contain an element with a value equal to <see cref="messageId" /> </exception>
    private bool SendPacketMemory(ulong entityId,
                                  byte messageId, Enums.GSS.Controllers controllerId,
                                  ref Memory<byte> packetMemory, Type? msgEnumType = null)
    {
        const int HeaderByteSize = 9;

        var serializedData = new Memory<byte>(new byte[HeaderByteSize + packetMemory.Length]);
        packetMemory.CopyTo(serializedData[HeaderByteSize..]);

        Serializer.WritePrimitive(entityId).CopyTo(serializedData);

        // Intentionally overwrite first byte of Entity ID
        Serializer.WritePrimitive((byte)controllerId).CopyTo(serializedData);
        Serializer.WritePrimitive(messageId).CopyTo(serializedData[8..]);


        if (msgEnumType == null)
        {
            Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}", Type,
                                   controllerId,
                                   entityId, messageId);
        }
        else
        {
            Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})", Type,
                                   controllerId,
                                   entityId,
                                   Enum.Parse(msgEnumType,
                                              Enum.GetName(msgEnumType, messageId) ??
                                              throw new
                                                  InvalidOperationException($"Message enum type {msgEnumType.FullName} has no element with a value of {messageId}")),
                                   messageId);
        }

        //Program.Logger.Verbose( "<--- Sending {0} bytes", packetData.Length );

        return Send(serializedData);
    }

    /// <summary>
    ///     Send data to the client
    /// </summary>
    /// <param name="packetData"></param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool Send(Memory<byte> packetData)
    {
        var hdrLen = 2;
        if (IsSequenced)
        {
            hdrLen += 2;
        }

        // TODO: Send UGSS messages that are split over RGSS
        while (packetData.Length > 0)
        {
            var len = Math.Min(packetData.Length + hdrLen, MaxPacketSize);

            var t = new Memory<byte>(new byte[len]);
            packetData[..(len - hdrLen)].CopyTo(t[hdrLen..]);

            if (IsSequenced)
            {
                if (IsReliable)
                {
                    Program.Logger.Verbose("<- {0} SeqNum =  {1}", Type, CurrentSequenceNumber);
                }

                Serializer.WritePrimitive(Utils.SimpleFixEndianness(CurrentSequenceNumber)).CopyTo(t.Slice(2, 2));
                unchecked { CurrentSequenceNumber++; }
            }

            var hdr = new GamePacketHeader(Type, 0, packetData.Length + hdrLen > MaxPacketSize, (ushort)t.Length);
            var hdrBytes = Serializer.WritePrimitive(Utils.SimpleFixEndianness(hdr.PacketHeader));
            hdrBytes.CopyTo(t);

            //Console.Write("< "+string.Concat(BitConverter.GetBytes(Utils.SimpleFixEndianness(hdr.PacketHeader)).ToArray().Select(b => b.ToString("X2")).ToArray()));
            //Console.WriteLine( " "+string.Concat( t.Slice(2).ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) )

            outgoingPackets.Enqueue(t);

            packetData = packetData[(len - hdrLen)..];
        }

        return true;
    }

    protected struct QueueItem
    {
        public Packet Packet;
        public GamePacketHeader Header;
    }
}