﻿using GameServer.Packets;
using Shared.Udp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameServer
{
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

                    for (int i = 0; i < dataTmp.Length; i++) // copy back
                        data[i] = dataTmp[i];

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
                    client.SendAck(Type, seqNum, packet.Recieved);
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
                p.CopyTo(t.Slice(1));
                Serializer.WritePrimitive((byte)msgID).CopyTo(t);
                p = t;
                Program.Logger.Verbose("<-- {0}: MsgID = {1} ({2})", Type, msgID, (byte)msgID);
            }
            else if (matMsgAttr != null)
            {
                var msgID = matMsgAttr.MsgID;
                var t = new Memory<byte>(new byte[1 + p.Length]);
                p.CopyTo(t.Slice(1));
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

            var controlMsgAttr = typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault() as ControlMessageAttribute;
            var matrixMsgAttr = typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault() as MatrixMessageAttribute;
            byte msgID;

            if (controlMsgAttr != null)
            {
                msgID = (byte)controlMsgAttr.MsgID;
            }
            else if (matrixMsgAttr != null)
            {
                msgID = (byte)matrixMsgAttr.MsgID;
            }
            else
            {
                throw new Exception();
            }

            var t = new Memory<byte>(new byte[1 + p.Length]);
            p.CopyTo(t.Slice(1));

            Serializer.WritePrimitive(msgID).CopyTo(t);

            p = t;

            if (msgEnumType == null)
            {
                Program.Logger.Verbose("<-- {0}: MsgID = 0x{1:X2}", Type, msgID);
            }
            else
            {
                Program.Logger.Verbose("<-- {0}: MsgID = {1} (0x{2:X2})", Type, Enum.Parse(msgEnumType, Enum.GetName(msgEnumType, msgID)), msgID);
            }

            return Send(p);
        }

        public bool SendGSS<T>(T pkt, ulong entityID, Enums.GSS.Controllers? controllerID = null, Type msgEnumType = null)
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

            var gssMsgAttr = typeof(T).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() as GSSMessageAttribute;

            if (gssMsgAttr != null)
            {
                var msgID = gssMsgAttr.MsgID;
                var t = new Memory<byte>(new byte[9 + p.Length]);
                p.CopyTo(t.Slice(9));

                Serializer.WritePrimitive(entityID).CopyTo(t);

                // Intentionally overwrite first byte of Entity ID
                if (controllerID.HasValue)
                {
                    Serializer.WritePrimitive((byte)controllerID.Value).CopyTo(t);
                }
                else if (gssMsgAttr.ControllerID.HasValue)
                {
                    Serializer.WritePrimitive((byte)gssMsgAttr.ControllerID.Value).CopyTo(t);
                }
                else
                {
                    throw new Exception();
                }

                Serializer.WritePrimitive(msgID).CopyTo(t.Slice(8));

                p = t;

                if (msgEnumType == null)
                {
                    Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}", Type, controllerID.HasValue ? controllerID.Value : gssMsgAttr.ControllerID.Value, entityID, msgID);
                }
                else
                {
                    Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})", Type, controllerID.HasValue ? controllerID.Value : gssMsgAttr.ControllerID.Value, entityID,
                                           Enum.Parse(msgEnumType, Enum.GetName(msgEnumType, msgID)), msgID);
                }
            }
            else
            {
                throw new Exception();
            }

            return Send(p);
        }

        public bool SendGSSClass<T>(T pkt, ulong entityID, Enums.GSS.Controllers? controllerID = null, Type msgEnumType = null)
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

            //Console.WriteLine( string.Concat( p.ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) );
            var gssMsgAttr = typeof(T).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault() as GSSMessageAttribute;

            if (gssMsgAttr != null)
            {
                var msgID = gssMsgAttr.MsgID;
                var t = new Memory<byte>(new byte[9 + p.Length]);
                p.CopyTo(t.Slice(9));

                Serializer.WritePrimitive(entityID).CopyTo(t);

                // Intentionally overwrite first byte of Entity ID
                if (controllerID.HasValue)
                {
                    Serializer.WritePrimitive((byte)controllerID.Value).CopyTo(t);
                }
                else if (gssMsgAttr.ControllerID.HasValue)
                {
                    Serializer.WritePrimitive((byte)gssMsgAttr.ControllerID.Value).CopyTo(t);
                }
                else
                {
                    throw new Exception();
                }

                Serializer.WritePrimitive(msgID).CopyTo(t.Slice(8));

                p = t;

                if (msgEnumType == null)
                {
                    Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}", Type, controllerID.HasValue ? controllerID.Value : gssMsgAttr.ControllerID.Value, entityID, msgID);
                }
                else
                {
                    Program.Logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})", Type, controllerID.HasValue ? controllerID.Value : gssMsgAttr.ControllerID.Value, entityID,
                                           Enum.Parse(msgEnumType, Enum.GetName(msgEnumType, msgID)), msgID);
                }

                //Program.Logger.Verbose( "<--- Sending {0} bytes", p.Length );
            }
            else
            {
                throw new Exception();
            }

            return Send(p);
        }

        public bool Send(Memory<byte> p)
        {
            var hdrLen = 2;
            if (IsSequenced)
            {
                hdrLen += 2;
            }

            // TODO: Send UGSS messages that are split over RGSS
            while (p.Length > 0)
            {
                var len = Math.Min(p.Length + hdrLen, MaxPacketSize);

                var t = new Memory<byte>(new byte[len]);
                p.Slice(0, len - hdrLen).CopyTo(t.Slice(hdrLen));

                if (IsSequenced)
                {
                    if (IsReliable)
                    {
                        Program.Logger.Verbose("<- {0} SeqNum =  {1}", Type, CurrentSequenceNumber);
                    }

                    Serializer.WritePrimitive(Utils.SimpleFixEndianness(CurrentSequenceNumber)).CopyTo(t.Slice(2, 2));
                    unchecked { CurrentSequenceNumber++; }
                }

                var hdr = new GamePacketHeader(Type, 0, p.Length + hdrLen > MaxPacketSize, (ushort)t.Length);
                var hdrBytes = Serializer.WritePrimitive(Utils.SimpleFixEndianness(hdr.PacketHeader));
                hdrBytes.CopyTo(t);

                //Console.Write("< "+string.Concat(BitConverter.GetBytes(Utils.SimpleFixEndianess(hdr.PacketHeader)).ToArray().Select(b => b.ToString("X2")).ToArray()));
                //Console.WriteLine( " "+string.Concat( t.Slice(2).ToArray().Select( b => b.ToString( "X2" ) ).ToArray() ) )

                outgoingPackets.Enqueue(t);

                p = p.Slice(len - hdrLen);
            }

            return true;
        }

        protected struct QueueItem
        {
            public Packet Packet;
            public GamePacketHeader Header;
        }
    }
}