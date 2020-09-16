using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Shared.Udp;
using MyGameServer.Packets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Linq;
using MyGameServer.Data;
using System.Diagnostics;

namespace MyGameServer {
	public class NetworkClient : INetworkClient, IInstance {
		public ulong InstanceID { get { return 0x4658119e70100000u; } }
		public Status NetClientStatus { get; protected set; }
		public uint SocketID { get; protected set; }
		public IPEndPoint RemoteEndpoint { get; protected set; }
		public DateTime NetLastActive { get; protected set; }
		protected IPacketSender Sender { get; set; }
		protected IPlayer Player { get; private set; }
		public ImmutableDictionary<ChannelType, Channel> NetChans { get; protected set; }

		public NetworkClient( IPEndPoint ep, uint socketID ) {
			SocketID = socketID;
			RemoteEndpoint = ep;
			NetClientStatus = Status.Unknown;
			NetLastActive = DateTime.Now;
		}

		public void Init( IPlayer player, IPacketSender sender ) {
			Player = player;
			Sender = sender;
			NetClientStatus = Status.Connecting;

			NetChans = Channel.GetChannels( this ).ToImmutableDictionary();
			NetChans[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
			NetChans[ChannelType.Matrix].PacketAvailable += Matrix_PacketAvailable;
			NetChans[ChannelType.ReliableGss].PacketAvailable += GSS_PacketAvailable;
			NetChans[ChannelType.UnreliableGss].PacketAvailable += GSS_PacketAvailable;
		}

		private void GSS_PacketAvailable( GamePacket packet ) {
			var ControllerID = packet.Read<Enums.GSS.Controllers>();
			Span<byte> entity = new byte[8];
			packet.Read( 7 ).ToArray().Reverse().ToArray().CopyTo( entity );
			var EntityID = BitConverter.ToUInt64(entity.ToArray());
			var MsgID = packet.Read<byte>();

			var conn = Controllers.Factory.Get(ControllerID);

			if( conn == null ) {
				Program.Logger.Verbose( "---> Unrecognized ControllerID for GSS Packet; Controller = {0} Entity = 0x{1:X8} MsgID = {2}!", ControllerID, EntityID, MsgID );
				Program.Logger.Warning( ">  {0}", BitConverter.ToString( packet.PacketData.ToArray() ).Replace( "-", " " ) );
				return;
			}

			//Program.Logger.Verbose("--> GSS; Controller = {0} Entity = 0x{1:X8} MsgID = {2}", ControllerID, EntityID, MsgID);
			conn.HandlePacket( this, Player, EntityID, MsgID, packet );

			// Character_BaseController = 115
			// Character_BaseController = 145
			// Character_BaseController = 148
			// Character_BaseController = 150

			// Character_BaseController = 179 ???
			// Character_BaseController = 180 ???
			// Character_BaseController = 181 ???
			// Character_BaseController = 182 ???
			// Character_BaseController = 140 ???
			// Character_BaseController = 216 ???
			// Character_BaseController = 142 ???

			// Character_CombatController = 138 ???
			// Character_CombatController = 134 ???
			// Character_CombatController = 118 ???
			// Character_CombatController = 119 ???
			// Character_CombatController = 120 ???
			// Character_CombatController = 121 ???
			// Character_CombatController = 122 ???
			// Character_CombatController = 123 ???
			// Character_CombatController = 124 ???
			// Character_CombatController = 125 ???
			// Character_CombatController = 126 ???

			// Vehicle_BaseController = 83 ???
			// Vehicle_BaseController = 86 ???
			// Vehicle_BaseController = 90 ???
		}

		private unsafe void Matrix_PacketAvailable( GamePacket packet ) {
			var msgID = packet.Read<Enums.MatrixPacketType>();
			Program.Logger.Verbose( "--> {0} ({1})", msgID, ((byte)msgID) );

			switch( msgID ) {
			case Enums.MatrixPacketType.Login:
				// Login
				var loginpkt = packet.Read<Packets.Matrix.Login>();
				Player.Login( loginpkt.CharacterGUID );
				
				break;
			case Enums.MatrixPacketType.EnterZoneAck:
				//Channels[ChannelType.ReliableGss].SendGSS( Test.GSS.ArcCompletionHistoryUpdate.GetEmpty(), InstanceID, msgEnumType: typeof( Enums.GSS.Generic.Events ) );
				//Channels[ChannelType.ReliableGss].SendGSS( new Packets.GSS.JobLedgerEntriesUpdate { Unk1 = 0x00 }, InstanceID, msgEnumType: typeof( Enums.GSS.Generic.Events ) );
				//Channels[ChannelType.ReliableGss].SendGSS( Test.GSS.ConfirmedPoseUpdate.GetPreSpawnConfirmedPoseUpdate(), Player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
				//Channels[ChannelType.ReliableGss].SendGSS( new Packets.GSS.TotalAchievementPoints { Number = 0x00000ad2 }, InstanceID, msgEnumType: typeof( Enums.GSS.Generic.Events ) );
				//Channels[ChannelType.ReliableGss].SendGSS( new Packets.GSS.InteractableStatusChanged { Unk1 = 0x0006eb22, Unk4 = 0x0100 }, InstanceID, msgEnumType: typeof( Enums.GSS.Generic.Events ) );

				Controllers.Factory.Get<Controllers.Character.BaseController>().Init( this, Player, this );

				break;
			case Enums.MatrixPacketType.KeyframeRequest:
				// TODO; See onKeyframeRequest in server_gamesocket.js
				var kfrpkt = packet.Read<Packets.Matrix.KeyFrameRequest>();

				int i=0;

				break;
			case Enums.MatrixPacketType.ClientStatus:
				var matStatus = new Packets.Matrix.MatrixStatus() {
					Unk = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
				};

				NetChans[ChannelType.Matrix].Send( matStatus );
				break;
			case Enums.MatrixPacketType.LogInstrumentation:
				// Ignore

				break;
			default:
				Program.Logger.Error( "---> Unrecognized Matrix Packet {0}[{1}]!!!", msgID, ((byte)msgID) );
				Program.Logger.Warning( ">  {0}", BitConverter.ToString( packet.PacketData.ToArray() ).Replace( "-", " " ) );
				break;
			}
		}

		private void Control_PacketAvailable( GamePacket packet ) {
			var msgID = packet.ReadBE<Enums.ControlPacketType>();
			Program.Logger.Verbose( "--> {0} ({1})", msgID, ((byte)msgID) );

			switch( msgID ) {
			case Enums.ControlPacketType.CloseConnection:
				var ccPkt = packet.Read<Packets.Control.CloseConnection>();
				// TODO: Cleanly dispose of client
				break;
			case Enums.ControlPacketType.MatrixAck:
				var mAckPkt = packet.Read<Packets.Control.MatrixAck>();
				// TODO: Track reliable packets
				break;
			case Enums.ControlPacketType.ReliableGSSAck:
				var gssAckPkt = packet.Read<Packets.Control.ReliableGSSAck>();
				// TODO: Track reliable packets
				break;
			case Enums.ControlPacketType.TimeSyncRequest:
				var req = packet.Read<Packets.Control.TimeSyncRequest>();
				var resp = new Packets.Control.TimeSyncResponse(req.ClientTime, (ulong)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()*1000u));
				NetChans[ChannelType.Control].Send( resp );
				break;
			case Enums.ControlPacketType.MTUProbe:
				var mtuPkt = packet.Read<Packets.Control.MTUProbe>();
				// TODO: ???
				break;
			default:
				Program.Logger.Error( "---> Unrecognized Control Packet {0} ({1:X2})!!!", msgID, ((byte)msgID) );
				Program.Logger.Warning( ">  {0}", BitConverter.ToString( packet.PacketData.ToArray() ).Replace( "-", " " ) );
				break;
			}
		}

		public void HandlePacket( Memory<byte> packet ) {
			if( NetClientStatus == Status.Connecting )
				NetClientStatus = Status.Connected;

			if( NetClientStatus != Status.Connected && NetClientStatus != Status.Idle )
				return;

			var idx = 0;
			var hdrSize = Unsafe.SizeOf<GamePacketHeader>();
			while( (idx + 2) < packet.Length ) {
				var hdr = Utils.ReadStructBE<GamePacketHeader>(packet.Slice(idx));

				if( hdr.Length == 0 || packet.Length < (hdr.Length + idx) )
					break;

				var p = new GamePacket(hdr, packet.Slice(idx + hdrSize, hdr.Length - hdrSize));

				//Program.Logger.Verbose("-> {0} = R:{1} S:{2} L:{3}", hdr.Channel, hdr.ResendCount, hdr.IsSplit, hdr.Length);

				NetChans[hdr.Channel].HandlePacket( p );

				idx += hdr.Length;
			}

			NetLastActive = DateTime.Now;
		}

		public virtual void NetworkTick( double deltaTime, double currTime ) {
			foreach( var c in NetChans.Values ) {
				c.Process();
			}
		}

		public void Send( Memory<byte> p ) {
			NetLastActive = DateTime.Now;

			var t = new Memory<byte>(new byte[4 + p.Length]);
			p.CopyTo( t.Slice( 4 ) );
			Utils.WriteStructBE( SocketID ).CopyTo( t );

			Sender.Send( t, RemoteEndpoint );
		}


		public void SendAck( ChannelType forChannel, ushort forSeqNum ) {
			if( forSeqNum == 0 )
				return;

			Program.Logger.Verbose( "<-- {0} Ack for {1} on {2}.", ChannelType.Control, forSeqNum, forChannel );

			if( forChannel == ChannelType.Matrix )
				NetChans[ChannelType.Control].SendBE( new Packets.Control.MatrixAck( NetChans[forChannel].CurrentSequenceNumber, forSeqNum ) );
			else if( forChannel == ChannelType.ReliableGss )
				NetChans[ChannelType.Control].SendBE( new Packets.Control.ReliableGSSAck( NetChans[forChannel].CurrentSequenceNumber, forSeqNum ) );
		}
	}
}
