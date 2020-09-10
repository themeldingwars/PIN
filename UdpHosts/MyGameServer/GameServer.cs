using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;
using MyGameServer.Packets;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Buffers;
using System.Net.Sockets;
using System.Net;

namespace MyGameServer {
	class GameServer : PacketServer {
		public delegate void SendPacketDelegate<T>( T pkt, IPEndPoint ep ) where T : struct;
		
		protected ConcurrentDictionary<uint, INetworkClient> Clients;
		public GameServer( ushort port ) : base(port) {
			Clients = new ConcurrentDictionary<uint, INetworkClient>();
		}

		protected override void Startup() {
			Test.DataUtils.Init();
			Controllers.Factory.Init();
		}
		protected override void Tick() {
		}
		protected override void NetworkTick() {
			foreach(var c in Clients.Values) {
				c.NetworkTick();
			}
		}
		protected override void Shutdown() {
		}

		protected override void HandlePacket( Packet packet ) {
			//Program.Logger.Information("[GAME] {0} sent {1} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
			//Program.Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));

			var socketID = packet.ReadBE<uint>();

			if( !Clients.ContainsKey(socketID) ) {
				var c = new NetworkClient(packet.RemoteEndpoint, socketID, this);
				c.Init();
				Clients.AddOrUpdate(socketID, c, ( id, nc ) => { return nc; });
			}

			Clients[socketID].HandlePacket(packet.PacketData.Slice(4));
		}
	}
}