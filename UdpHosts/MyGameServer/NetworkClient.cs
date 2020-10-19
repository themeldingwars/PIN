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
using System.Threading;

namespace MyGameServer {
	public class NetworkClient : INetworkClient {
		public Status NetClientStatus { get; protected set; }
		public uint SocketID { get; protected set; }
		public IPEndPoint RemoteEndpoint { get; protected set; }
		public DateTime NetLastActive { get; protected set; }
		protected IPacketSender Sender { get; set; }
		protected IPlayer Player { get; private set; }
		public ImmutableDictionary<ChannelType, Channel> NetChans { get; protected set; }
		public IShard AssignedShard { get; protected set; }

		public NetworkClient( IPEndPoint ep, uint socketID ) {
			SocketID = socketID;
			RemoteEndpoint = ep;
			NetClientStatus = Status.Unknown;
			NetLastActive = DateTime.Now;
		}

		public void Init( IPlayer player, IShard shard, IPacketSender sender ) {
			Player = player;
			Sender = sender;
			NetClientStatus = Status.Connecting;
			AssignedShard = shard;

			NetChans = Channel.GetChannels( this ).ToImmutableDictionary();
			NetChans[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
			NetChans[ChannelType.Matrix].PacketAvailable += Matrix_PacketAvailable;
			NetChans[ChannelType.ReliableGss].PacketAvailable += GSS_PacketAvailable;
			NetChans[ChannelType.UnreliableGss].PacketAvailable += GSS_PacketAvailable;
		}

		private void GSS_PacketAvailable( GamePacket packet ) {
			var ControllerID = packet.Read<Enums.GSS.Controllers>();
			Span<byte> entity = stackalloc byte[8];
			packet.Read( 7 ).ToArray().CopyTo( entity );
			var EntityID = unchecked(BitConverter.ToUInt64(entity) << 8);
			var MsgID = packet.Read<byte>();

			var conn = Controllers.Factory.Get(ControllerID);

			if( conn == null ) {
				Program.Logger.Verbose( "---> Unrecognized ControllerID for GSS Packet; Controller = {0} Entity = 0x{1:X16} MsgID = {2}!", ControllerID, EntityID, MsgID );
				Program.Logger.Warning( ">  {0}", BitConverter.ToString( packet.PacketData.ToArray() ).Replace( "-", " " ) );
				return;
			}

			Program.Logger.Verbose("--> {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3}", packet.Header.Channel, ControllerID, EntityID, MsgID);
			conn.HandlePacket( this, Player, EntityID, MsgID, packet );
		}

		private void Matrix_PacketAvailable( GamePacket packet ) {
			var msgID = packet.Read<Enums.MatrixPacketType>();
			Program.Logger.Verbose("--> {0}: MsgID = {1} ({2})", ChannelType.Matrix, msgID, ((byte)msgID) );

			switch( msgID ) {
			case Enums.MatrixPacketType.Login:
				// Login
				var loginpkt = packet.Read<Packets.Matrix.Login>();
				Player.Login( loginpkt.CharacterGUID );

				break;
			case Enums.MatrixPacketType.EnterZoneAck:
				Controllers.Factory.Get<Controllers.Character.BaseController>().Init( this, Player, AssignedShard );

				break;
			case Enums.MatrixPacketType.KeyframeRequest:
				// TODO; See onKeyframeRequest in server_gamesocket.js
				var kfrpkt = packet.Read<Packets.Matrix.KeyFrameRequest>();

				int i=0;

				break;
			case Enums.MatrixPacketType.ClientStatus:
				NetChans[ChannelType.Matrix].SendClass(new Packets.Matrix.MatrixStatus() );
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
			var msgID = packet.Read<Enums.ControlPacketType>();
			Program.Logger.Verbose("--> {0}: MsgID = {1} ({2})", ChannelType.Control, msgID, ((byte)msgID) );

			switch( msgID ) {
			case Enums.ControlPacketType.CloseConnection:
				var ccPkt = packet.Read<Packets.Control.CloseConnection>();
				// TODO: Cleanly dispose of client
				break;
			case Enums.ControlPacketType.MatrixAck:
				var mAckPkt = packet.Read<Packets.Control.MatrixAck>();
				Program.Logger.Verbose( "--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianess( mAckPkt.AckFor), ChannelType.Matrix );
				// TODO: Track reliable packets
				break;
			case Enums.ControlPacketType.ReliableGSSAck:
				var gssAckPkt = packet.Read<Packets.Control.ReliableGSSAck>();
				Program.Logger.Verbose( "--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianess( gssAckPkt.AckFor), ChannelType.ReliableGss );
				// TODO: Track reliable packets
				break;
			case Enums.ControlPacketType.TimeSyncRequest:
				var req = packet.Read<Packets.Control.TimeSyncRequest>();

				NetChans[ChannelType.Control].Send( new Packets.Control.TimeSyncResponse( req.ClientTime, unchecked(AssignedShard.CurrentTimeLong*1000) ) );
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

		public void HandlePacket( ReadOnlyMemory<byte> data, Packet packet ) {
			if( NetClientStatus == Status.Connecting )
				NetClientStatus = Status.Connected;

			if( NetClientStatus != Status.Connected && NetClientStatus != Status.Idle )
				return;

			var idx = 0;
			var hdrSize = Unsafe.SizeOf<GamePacketHeader>();
			while( (idx + 2) < data.Length ) {
				var hdr = Utils.ReadStruct<GamePacketHeader>(data.Slice(idx,2).ToArray().Reverse().ToArray().AsMemory());

				if( hdr.Length == 0 || data.Length < (hdr.Length + idx) )
					break;

				var p = new GamePacket(hdr, data.Slice(idx + hdrSize, hdr.Length - hdrSize), packet.Recieved);

				//Program.Logger.Verbose("-> {0} = R:{1} S:{2} L:{3}", hdr.Channel, hdr.ResendCount, hdr.IsSplit, hdr.Length);

				NetChans[hdr.Channel].HandlePacket( p );

				idx += hdr.Length;
			}

			NetLastActive = DateTime.Now;
		}

		public virtual void NetworkTick( double deltaTime, ulong currTime, CancellationToken ct ) {
			foreach( var c in NetChans.Values ) {
				c.Process(ct);
			}
		}

		public void Send( Memory<byte> p ) {
			NetLastActive = DateTime.Now;

			var t = new Memory<byte>(new byte[4 + p.Length]);
			p.CopyTo( t.Slice( 4 ) );
			Utils.WriteStruct( Utils.SimpleFixEndianess(SocketID) ).CopyTo( t );

			Sender.Send( t, RemoteEndpoint );
		}


		public void SendAck( ChannelType forChannel, ushort forSeqNum, DateTime? recvd = null ) {
			if( recvd != null )
				Program.Logger.Verbose( "<-- {0} Ack for {1} on {2} after {3}ms.", ChannelType.Control, forSeqNum, forChannel, (DateTime.Now - recvd.Value).TotalMilliseconds );
			else
				Program.Logger.Verbose( "<-- {0} Ack for {1} on {2}.", ChannelType.Control, forSeqNum, forChannel );

			var forNum = Utils.SimpleFixEndianess( forSeqNum );
			var nextNum = Utils.SimpleFixEndianess(  unchecked((ushort)(forSeqNum+1)) );

			if( forChannel == ChannelType.Matrix )
				NetChans[ChannelType.Control].SendClass( new Packets.Control.MatrixAck { AckFor = forNum, NextSeqNum = nextNum } );
			else if( forChannel == ChannelType.ReliableGss )
				NetChans[ChannelType.Control].SendClass( new Packets.Control.ReliableGSSAck { AckFor = forNum, NextSeqNum = nextNum } );
		}
	}
}
