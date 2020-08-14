using System;
using System.Collections.Generic;
using System.Text;

using ServerShared;
using MyGameServer.Packets;
using System.Linq;
using System.Collections.Concurrent;

namespace MyGameServer {
	public enum ChannelType : byte {
		Control = 0,
		Matrix = 1,
		ReliableGss = 2,
		UnreliableGss = 3,
	}

	public class Channel {
		protected struct QueueItem {
			public Packet Packet;
			public GamePacketHeader Header;
		}
		private static readonly byte[] xorByte = new byte[] { 0x00, 0xFF, 0xCC, 0xAA };
		private static readonly ulong[] xorULong = new ulong[] { 0x00, 0xFFFFFFFFFFFFFFFF, 0xCCCCCCCCCCCCCCCC, 0xAAAAAAAAAAAAAAAA };

		public static Dictionary<ChannelType, Channel> GetChannels( INetworkClient client ) {
			return new Dictionary<ChannelType, Channel>() {
				{ ChannelType.Control, new Channel(ChannelType.Control, false, false, client) },
				{ ChannelType.Matrix, new Channel(ChannelType.Matrix, true, true, client) },
				{ ChannelType.ReliableGss, new Channel(ChannelType.ReliableGss, true, true, client) },
				{ ChannelType.UnreliableGss, new Channel(ChannelType.UnreliableGss, true, false, client) },
			};
		}

		public ChannelType Type { get; protected set; }
		public bool IsSequenced { get; protected set; }
		public bool IsReliable { get; protected set; }
		public ushort CurrentSequenceNumber { get; protected set; }

		public delegate void PacketAvailableDelegate( GamePacket packet );
		public event PacketAvailableDelegate PacketAvailable;

		protected INetworkClient client;
		protected ConcurrentQueue<GamePacket> incomingPackets;
		protected ConcurrentQueue<Memory<byte>> outgoingPackets;

		protected Channel( ChannelType ct, bool isSequenced, bool isReliable, INetworkClient c ) {
			Type = ct;
			IsSequenced = isSequenced;
			IsReliable = isReliable;
			client = c;
			CurrentSequenceNumber = 1;

			incomingPackets = new ConcurrentQueue<GamePacket>();
			outgoingPackets = new ConcurrentQueue<Memory<byte>>();
		}

		public void HandlePacket( GamePacket packet ) {
			incomingPackets.Enqueue(packet);
		}

		public void Process() {
			while( incomingPackets.TryDequeue(out GamePacket packet) ) {

				ushort seqNum = 0;
				if( IsSequenced ) {
					seqNum = packet.ReadBE<ushort>();
					// TODO: Verify? re-order?
				}

				if( packet.Header.ResendCount > 0 ) {
					// de-xor data
					int x = packet.PacketData.Length >> 3;

					if( x > 0 ) {
						Span<ulong> uSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ulong>(packet.PacketData.Span);

						for( int i = 0; i < x; i++ )
							uSpan[i] ^= xorULong[packet.Header.ResendCount];
					}

					for( int i = x * 8; i < packet.PacketData.Length; i++ )
						packet.PacketData.Span[i] ^= xorByte[packet.Header.ResendCount];
				}

				if( IsReliable )
					client.SendAck(Type, seqNum);

				PacketAvailable?.Invoke(packet);
			}

			while(outgoingPackets.TryDequeue(out Memory<byte> qi) ) {
				client.Send(qi);
				Program.Logger.Verbose("<--- {0}: {1} bytes", Type, qi.Length);
			}
		}

		public void Send<T>( T pkt ) where T : struct {
			Memory<byte> p;
			if( pkt is IWritableStruct write ) {
				p = write.Write();
			} else
				p = Utils.WriteStruct(pkt);

			var conMsgAttr = (typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault()) as ControlMessageAttribute;
			var matMsgAttr = (typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault()) as MatrixMessageAttribute;

			if( conMsgAttr != null ) {
				var msgID = conMsgAttr.MsgID;
				var t = new Memory<byte>(new byte[1 + p.Length]);
				p.CopyTo(t.Slice(1));
				Utils.WriteStruct((byte)msgID).CopyTo(t);
				p = t;
				Program.Logger.Verbose("<-- {0}: {1} ({2})", Type, msgID, (byte)msgID);
			} else if( matMsgAttr != null ) {
				var msgID = matMsgAttr.MsgID;
				var t = new Memory<byte>(new byte[1 + p.Length]);
				p.CopyTo(t.Slice(1));
				Utils.WriteStruct((byte)msgID).CopyTo(t);
				p = t;
				Program.Logger.Verbose("<-- {0}: {1} ({2})", Type, msgID, (byte)msgID);
			} else
				throw new Exception();

			Send(p);
		}

		public void SendBE<T>( T pkt ) where T : struct {
			Memory<byte> p;
			if( pkt is IWritableStruct write ) {
				p = write.WriteBE();
			} else
				p = Utils.WriteStructBE(pkt);

			var conMsgAttr = (typeof(T).GetCustomAttributes(typeof(ControlMessageAttribute), false).FirstOrDefault()) as ControlMessageAttribute;
			var matMsgAttr = (typeof(T).GetCustomAttributes(typeof(MatrixMessageAttribute), false).FirstOrDefault()) as MatrixMessageAttribute;

			if( conMsgAttr != null ) {
				var msgID = conMsgAttr.MsgID;
				var t = new Memory<byte>(new byte[1 + p.Length]);
				p.CopyTo(t.Slice(1));
				Utils.WriteStructBE((byte)msgID).CopyTo(t);
				p = t;
				Program.Logger.Verbose("<-- {0}: {1} ({2})", Type, msgID, (byte)msgID);
			} else if( matMsgAttr != null ) {
				var msgID = matMsgAttr.MsgID;
				var t = new Memory<byte>(new byte[1 + p.Length]);
				p.CopyTo(t.Slice(1));
				Utils.WriteStructBE((byte)msgID).CopyTo(t);
				p = t;
				Program.Logger.Verbose("<-- {0}: {1} ({2})", Type, msgID, (byte)msgID);
			} else
				throw new Exception();

			SendBE(p);
		}

		public void SendGSS<T>( T pkt, byte? controllerID = null, ulong? entityID = null ) where T : struct {
			Memory<byte> p;
			if( pkt is IWritableStruct write ) {
				p = write.Write();
			} else
				p = Utils.WriteStruct(pkt);

			var gssMsgAttr = (typeof(T).GetCustomAttributes(typeof(GSSMessageAttribute), false).FirstOrDefault()) as GSSMessageAttribute;

			if( gssMsgAttr != null ) {
				var msgID = gssMsgAttr.MsgID;
				var t = new Memory<byte>(new byte[9 + p.Length]);
				p.CopyTo(t.Slice(9));

				if( entityID.HasValue )
					Utils.WriteStruct(entityID.Value).CopyTo(t);
				else if( gssMsgAttr.EntityID.HasValue )
					Utils.WriteStruct(gssMsgAttr.EntityID.Value).CopyTo(t);
				else
					throw new Exception();

				// Intentionally overwrite first byte of Entity ID
				if( controllerID.HasValue )
					Utils.WriteStruct(controllerID.Value).CopyTo(t);
				else if( gssMsgAttr.ControllerID.HasValue )
					Utils.WriteStruct(gssMsgAttr.ControllerID.Value).CopyTo(t);
				else
					throw new Exception();

				Utils.WriteStruct((byte)msgID).CopyTo(t.Slice(8));

				p = t;

				Program.Logger.Verbose("<-- {0}: {1} - {2} - {3} ({4})", Type, controllerID.HasValue ? controllerID.Value : gssMsgAttr.ControllerID.Value, entityID.HasValue ? entityID.Value : gssMsgAttr.EntityID.Value, msgID, (byte)msgID);
			} else
				throw new Exception();

			Send(p);
		}

		public void Send( Memory<byte> p ) {
			var extraLen = 2;
			if( IsSequenced )
				extraLen += 2;

			var hdr = new GamePacketHeader(Type, 0, false, (ushort)(p.Length + extraLen));
			var t = new Memory<byte>(new byte[p.Length + extraLen]);
			p.CopyTo(t.Slice(extraLen));
			Utils.WriteStructBE(hdr).CopyTo(t);

			if( IsSequenced ) {
				Utils.WriteStructBE(CurrentSequenceNumber).CopyTo(t.Slice(extraLen - 2));
				unchecked { CurrentSequenceNumber++; }
			}

			p = t;

			outgoingPackets.Enqueue(p);
		}

		public void SendBE( Memory<byte> p ) {
			var extraLen = 2;
			if( IsSequenced )
				extraLen += 2;

			var hdr = new GamePacketHeader(Type, 0, false, (ushort)(p.Length + extraLen));
			var t = new Memory<byte>(new byte[p.Length + extraLen]);
			p.CopyTo(t.Slice(extraLen));
			Utils.WriteStructBE(hdr).CopyTo(t);

			if( IsSequenced ) {
				Utils.WriteStructBE(CurrentSequenceNumber).CopyTo(t.Slice(extraLen - 2));
				unchecked { CurrentSequenceNumber++; }
			}

			p = t;

			outgoingPackets.Enqueue(p);
		}
	}
}