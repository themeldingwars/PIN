using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameServer.Packets;
using GameServer.Packets.Game;
using GameServer.Packets.Matrix;

namespace GameServer
{
    internal class GameServer
    {
        public void Run()
        {
            while (true)
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var receivedData = ListenForData(ref remoteEndPoint);
                var responseData = ProcessData(receivedData);
                SendData(responseData, ref remoteEndPoint);
            }
        }

        private static void SendData(byte[] data, ref IPEndPoint remoteEndPoint)
        {
            using (var responseConnection = new UdpClient(remoteEndPoint.Address.ToString(), remoteEndPoint.Port))
            {
                try
                {
                    Console.WriteLine($"Send data to {remoteEndPoint}:");
                    Console.WriteLine($"{Encoding.ASCII.GetString(data, 0, data.Length)}");
                    responseConnection.Send(data, data.Length);
                }
                catch (SocketException socketException)
                {
                    Console.WriteLine(socketException);
                }
                finally
                {
                    responseConnection.Close();
                }
            }
        }

        private static byte[] ProcessData(byte[] data)
        {
            var parser = new PacketParser();
            var incomingPacket = parser.ParseIncomingData(data);
            var outgoingPacket = ParseUdpPacket(incomingPacket);
            return outgoingPacket.ToBytes();
        }

        private static byte[] ListenForData(ref IPEndPoint remoteEndPoint)
        {
            const int ListenPort = 25000;
            var data = new byte[] { };

            using (var receiveConnection = new UdpClient(ListenPort))
            {
                try
                {
                    Console.WriteLine($"Listening on {ListenPort} - waiting for data...");
                    data = receiveConnection.Receive(ref remoteEndPoint);

                    Console.WriteLine($"Received data from {remoteEndPoint}:");
                    Console.WriteLine($"{Encoding.ASCII.GetString(data, 0, data.Length)}");
                }
                catch (SocketException socketException)
                {
                    Console.WriteLine(socketException);
                }
                finally
                {
                    receiveConnection.Close();
                }
            }

            return data;
        }

        private static Packet ParseUdpPacket(Packet incomingPacket)
        {
            return incomingPacket switch
                   {
                       MatrixPacket matrixPacket => ParseMatrixPacket(matrixPacket),
                       GamePacket gamePacket     => ParseGamePacket(gamePacket),
                       _                         => throw new UnknownPacketException()
                   };
        }

        private static MatrixPacket ParseMatrixPacket(MatrixPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case PokeMatrixPacket pokePacket:
                    Console.WriteLine($"Client uses protocol version: {pokePacket.ProtocolVersion}");
                    return new HeheMatrixPacket
                           {
                               SocketId = 0u,
                               Type = Encoding.ASCII.GetBytes("HEHE"),
                               NextSocketId = 1337u
                           };
                case KissMatrixPacket kissPacket:
                    Console.WriteLine($"Client uses next socket id {kissPacket.NextSocketId} and stream protocol version {kissPacket.StreamingProtocolVersion}");
                    return new HuggMatrixPacket
                           {
                               SocketId = 0u,
                               Type = Encoding.ASCII.GetBytes("HUGG"),
                               SequenceStart = 54321,
                               GameServerPort = 25000
                           };
                default:
                    throw new UnknownMatrixPacketException(Encoding.ASCII.GetString(incomingPacket.Type));
            }
        }

        private static GamePacket ParseGamePacket(GamePacket incomingPacket)
        {
            throw new NotImplementedException();
        }
    }
}