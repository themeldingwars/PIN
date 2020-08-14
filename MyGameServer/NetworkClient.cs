using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using ServerShared;
using MyGameServer.Packets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Linq;
using MyGameServer.Data;

namespace MyGameServer {
	public class NetworkClient : INetworkClient {
		public const ulong InstanceID = 0x4658119e70100000u; // 0xFB0010709E115846
		public Status ClientStatus { get; protected set; }
		public uint SocketID { get; protected set; }
		public IPEndPoint RemoteClient { get; protected set; }
		public DateTime LastActive { get; protected set; }
		protected IPacketSender Sender { get; set; }
		public ImmutableDictionary<ChannelType, Channel> Channels { get; protected set; }

		public Player Player { get; protected set; }
		public Zone Zone { get; protected set; }

		public NetworkClient( IPEndPoint ep, uint socketID, IPacketSender sender ) {
			SocketID = socketID;
			RemoteClient = ep;
			ClientStatus = Status.Unknown;
			Sender = sender;
			LastActive = DateTime.Now;

			Player = new Player(this);
			Zone = Test.DataUtils.GetZone(448); 
		}

		public void Init() {
			Channels = Channel.GetChannels(this).ToImmutableDictionary();
			Channels[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
			Channels[ChannelType.Matrix].PacketAvailable += Matrix_PacketAvailable;
			Channels[ChannelType.ReliableGss].PacketAvailable += GSS_PacketAvailable;
			Channels[ChannelType.UnreliableGss].PacketAvailable += GSS_PacketAvailable;

			ClientStatus = Status.Connecting;
		}

		private void GSS_PacketAvailable( GamePacket packet ) {
			var ControllerID = packet.Read<Packets.GSS.Controllers>();
			Span<byte> entity = new byte[8];
			packet.Read(7).ToArray().Reverse().ToArray().CopyTo(entity);
			var EntityID = BitConverter.ToUInt64(entity.ToArray());
			var MsgID = packet.Read<byte>();

			var conn = Controllers.ControllerFactory.Get(ControllerID);

			if( conn == null ) {
				Program.Logger.Verbose("---> Unrecognized ControllerID for GSS Packet; Controller = {0} Entity = 0x{1:X8} MsgID = {2}!", ControllerID, EntityID, MsgID);
				return;
			}

			Program.Logger.Verbose("--> GSS; Controller = {0} Entity = 0x{1:X8} MsgID = {2}", ControllerID, EntityID, MsgID);
			conn.HandlePacket(this, EntityID, MsgID, packet);
		}

		private unsafe void Matrix_PacketAvailable( GamePacket packet ) {
			var msgID = packet.Read<MatrixPacketType>();
			Program.Logger.Verbose("--> {0} ({1})", msgID, ((byte)msgID));

			switch(msgID) {
			case MatrixPacketType.Login:
				Program.Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
				// Login
				var pkt = packet.Read<Packets.Matrix.Login>();
				Player.Login(pkt.CharacterGUID);
				Player.Character.Position = Zone.POIs["watchtower"];

				// WelcomeToTheMatrix
				var wel = new Packets.Matrix.WelcomeToTheMatrix(InstanceID, 0, 0);
				Channels[ChannelType.Matrix].Send(wel);

				// EnterZone
				var enterZone = new Packets.Matrix.EnterZone();
				enterZone.InstanceId = InstanceID;// | 448; // 0x4658119E701000FB; // 0xFB0010709E115846u;
				enterZone.ZoneId = Zone.ID;
				enterZone.ZoneTimestamp = Zone.Timestamp;
				enterZone.PreviewModeFlag = 0;
				enterZone.ZoneOwner = "r5_exec";
				enterZone.Unk1 = new byte[] { 0x5F, 0x4C, 0xF5, 0xC9, 0x01, 0x00 };
				enterZone.HotfixLevel = 0;
				enterZone.MatchId = 0;
				enterZone.Unk2 = new byte[] { 0x00, 0x5e, 0xdb, 0xe2, 0x63 };
				enterZone.ZoneName = Zone.Name;
				enterZone.Unk3 = 0;
				enterZone.Unk_ZoneTime = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x41, 0x7A, 0x7D, 0x65, 0x3F };
				enterZone.Unk4 = 0x2894E7951D410500u;
				enterZone.Unk5 = 0xEA5BD613F0400500u;
				enterZone.Unk6 = 0x000000000000F03Fu;
				enterZone.Unk7 = 0x0000000000000000u;
				enterZone.Unk8 = 0x0000000000000000u;
				enterZone.Unk9 = 0;
				enterZone.SpectatorModeFlag = 0;

				Channels[ChannelType.Matrix].Send(enterZone);

				/*var syncReq = new Packets.Matrix.SynchronizationRequest() {
					Unk = new byte[] {
						0xb4, 0x50, 0xaf, 0xa8, 0x1c, 0xa5, 0x45, 0x15, 0x00, 0x0d, 0x0c, 0x0c, 0xcc, 0xcc, 0xc6, 0x1b,
						0xcd, 0x05, 0xc6, 0xcc, 0xcc, 0xcc, 0x21, 0xe3, 0x9e, 0x37, 0x71, 0x0d, 0x3f, 0xb9, 0xb4, 0xd6,
						0x9c, 0x20, 0x36, 0xf7, 0x4a, 0x01
					}
				};

				Channels[ChannelType.Matrix].Send(syncReq);*/
				break;
			case MatrixPacketType.EnterZoneAck:
				Channels[ChannelType.ReliableGss].SendGSS(Test.GSS.ArcCompletionHistoryUpdate.GetEmpty(), 0, InstanceID);
				Channels[ChannelType.ReliableGss].SendGSS(new Packets.GSS.JobLedgerEntriesUpdate { Unk1 = 0x00 }, 0, InstanceID);

				break;
			case MatrixPacketType.KeyframeRequest:
				break;
			case MatrixPacketType.ClientStatus:
				var matStatus = new Packets.Matrix.MatrixStatus() {
					Unk = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
				};

				Channels[ChannelType.Matrix].Send(matStatus);
				break;
			default:
				Program.Logger.Error("---> Unrecognized Matrix Packet {0}[{1}]!!!", msgID, ((byte)msgID));
				Program.Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
				break;
			}
		}

		private void Control_PacketAvailable( GamePacket packet ) {
			var msgID = packet.ReadBE<ControlPacketType>();
			Program.Logger.Verbose("--> {0} ({1})", msgID, ((byte)msgID));

			switch( msgID ) {
			case ControlPacketType.CloseConnection:
				var ccPkt = packet.Read<Packets.Control.CloseConnection>();
				// TODO: Cleanly dispose of client
				break;
			case ControlPacketType.MatrixAck:
				var mAckPkt = packet.Read<Packets.Control.MatrixAck>();
				// TODO: Track reliable packets
				break;
			case ControlPacketType.ReliableGSSAck:
				var gssAckPkt = packet.Read<Packets.Control.ReliableGSSAck>();
				// TODO: Track reliable packets
				break;
			case ControlPacketType.TimeSyncRequest:
				var req = packet.Read<Packets.Control.TimeSyncRequest>();
				var resp = new Packets.Control.TimeSyncResponse(req.ClientTime, (ulong)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()*1000u));
				Channels[ChannelType.Control].Send(resp);
				break;
			case ControlPacketType.MTUProbe:
				var mtuPkt = packet.Read<Packets.Control.MTUProbe>();
				// TODO: ???
				break;
			default:
				Program.Logger.Error("---> Unrecognized Control Packet [{0}]!!!", ((byte)msgID));
				Program.Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
				break;
			}
		}

		public void HandlePacket( Memory<byte> packet ) {
			if( ClientStatus == Status.Connecting )
				ClientStatus = Status.Connected;

			if( ClientStatus != Status.Connected && ClientStatus != Status.Idle )
				return;

			var idx = 0;
			var hdrSize = Unsafe.SizeOf<GamePacketHeader>();
			while( (idx + 2) < packet.Length ) {
				var hdr = Utils.ReadStructBE<GamePacketHeader>(packet.Slice(idx));
				
				if( hdr.Length == 0 || packet.Length < (hdr.Length + idx) )
					break;

				var p = new GamePacket(hdr, packet.Slice(idx + hdrSize, hdr.Length - hdrSize));

				//Program.Logger.Verbose("-> {0} = R:{1} S:{2} L:{3}", hdr.Channel, hdr.ResendCount, hdr.IsSplit, hdr.Length);

				Channels[hdr.Channel].HandlePacket(p);

				idx += hdr.Length;
			}

			LastActive = DateTime.Now;
		}

		public void NetworkTick() {
			foreach(var c in Channels.Values) {
				c.Process();
			}
		}

		public void Send( Memory<byte> p ) {
			LastActive = DateTime.Now;

			var t = new Memory<byte>(new byte[4 + p.Length]);
			p.CopyTo(t.Slice(4));
			Utils.WriteStructBE(SocketID).CopyTo(t);

			Sender.Send(t, RemoteClient);
		}


		public void SendAck( ChannelType forChannel, ushort forSeqNum ) {
			if( forSeqNum == 0 )
				return;

			Program.Logger.Verbose("<-- {0} Ack for {1} on {2}.", ChannelType.Control, forSeqNum, forChannel);

			if( forChannel == ChannelType.Matrix )
				Channels[ChannelType.Control].SendBE(new Packets.Control.MatrixAck(Channels[forChannel].CurrentSequenceNumber, forSeqNum));
			else if( forChannel == ChannelType.ReliableGss )
				Channels[ChannelType.Control].SendBE(new Packets.Control.ReliableGSSAck(Channels[forChannel].CurrentSequenceNumber, forSeqNum));
		}
	}
}
