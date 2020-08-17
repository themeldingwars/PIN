using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bitter;

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

        private byte[] ProcessData(byte[] data)
        {
            using (var stringStream = new MemoryStream(data))
            using (var inputBinaryStream = new BinaryStream(stringStream))
            using (var outputBinaryStream = ParseUdpPacket(inputBinaryStream))
            {
                outputBinaryStream.ByteOffset = 0;
                data = outputBinaryStream.Read.ByteArray((int) outputBinaryStream.Length);
                Console.WriteLine($"Response data {Encoding.ASCII.GetString(data, 0, data.Length)}");
                return data;
            }
        }

        private byte[] ListenForData(ref IPEndPoint remoteEndPoint)
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

        private BinaryStream ParseUdpPacket(BinaryStream inputBinaryStream)
        {
            BinaryStream outputBinaryStream = null;

            while (!inputBinaryStream.EndOfStream)
            {
                var socketId = inputBinaryStream.Read.UInt();

                if (socketId == 0)
                {
                    Console.WriteLine("Detected Matrix packet");
                    outputBinaryStream = ParseMatrixPacket(inputBinaryStream);
                }
            }

            return outputBinaryStream;
        }

        private BinaryStream ParseMatrixPacket(BinaryStream inputBinaryStream)
        {
            var packetType = inputBinaryStream.Read.String(4);
            Console.WriteLine($"Matrix packet type: {packetType}");

            var response = new BinaryStream();

            switch (packetType)
            {
                case "POKE":
                    Console.WriteLine($"Client uses protocol version: {inputBinaryStream.Read.UShort()}");
                    response.Write.UInt(0u);
                    response.Write.ByteArray(Encoding.ASCII.GetBytes("HEHE"));
                    response.Write.UInt(1337u);
                    break;
                case "KISS":
                    Console.WriteLine($"Client uses socket id {inputBinaryStream.Read.UInt()} and protocol version {inputBinaryStream.Read.UInt()}");
                    response.Write.UInt(0u);
                    response.Write.ByteArray(Encoding.ASCII.GetBytes("HUGG"));
                    response.Write.UShort(54321);
                    response.Write.UShort(25000);
                    break;
                case "ABRT":
                default:
                    break;
            }

            return response;
        }
    }
}