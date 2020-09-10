using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyGameServer.old {
	class ClientConnection {
		public uint SocketID { get; private set; }
		public uint SendSeqNum { get; set; }
		public uint RecvSeqNum { get; set; }
		public Socket ClientSocket { get; private set; }
		public IPEndPoint ClientEndpoint { get; private set; }
		public IPEndPoint ServerEndpoint { get; private set; }

		public ClientConnection(uint socketID, int port, IPEndPoint client) {
			SocketID = socketID;
			SendSeqNum = 1;
			RecvSeqNum = 1;
			ClientEndpoint = client;

			ClientSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			ServerEndpoint = new IPEndPoint(IPAddress.Loopback, port);
		}

		public void Start() {
			ClientSocket.Blocking = true;
			ClientSocket.ReceiveBufferSize = PacketServer.MTU * 5;
			ClientSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.PacketInformation, true );
			ClientSocket.Bind( ServerEndpoint );
		}
	}
}
