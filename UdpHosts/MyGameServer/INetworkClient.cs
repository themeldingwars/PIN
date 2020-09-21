using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Shared.Udp;
using MyGameServer.Packets;
using System.Collections.Immutable;

namespace MyGameServer {
	public enum Status {
		Unknown = 0,
		Connecting = 1,
		Connected,
		Idle,
		Disconnecting,
		Aborted
	}

	public interface INetworkClient {
		Status NetClientStatus { get; }
		uint SocketID { get; }
		IPEndPoint RemoteEndpoint { get; }
		DateTime NetLastActive { get; }
		ImmutableDictionary<ChannelType, Channel> NetChans { get; }

		void Init( IPlayer player, IShard shard, IPacketSender sender);
		void HandlePacket( Memory<byte> packet );
		void NetworkTick( double deltaTime, double currTime );
		void Send( Memory<byte> p );
		void SendAck( ChannelType forChannel, ushort forSeqNumber );
	}
}
