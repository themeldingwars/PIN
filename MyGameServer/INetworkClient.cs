using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using ServerShared;
using MyGameServer.Packets;

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
		Status ClientStatus { get; }
		uint SocketID { get; }
		IPEndPoint RemoteClient { get; }
		DateTime LastActive { get; }

		void Init();
		void HandlePacket( Memory<byte> packet );
		void NetworkTick();
		void Send( Memory<byte> p );
		void SendAck( ChannelType forChannel, ushort forSeqNumber );
	}
}
