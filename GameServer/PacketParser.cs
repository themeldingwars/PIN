using System;
using System.IO;
using System.Text;
using Bitter;
using GameServer.Packets;
using GameServer.Packets.Game;
using GameServer.Packets.Matrix;

namespace GameServer
{
    public class PacketParser
    {
        private uint _socketId;

        public Packet ParseIncomingData(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var binaryStream = new BinaryStream(memoryStream))
            {
                _socketId = binaryStream.Read.UInt();

                if (_socketId == 0)
                {
                    return ParseMatrixPacket(binaryStream);
                }

                return ParseGamePacket(binaryStream);
            }
        }

        private MatrixPacket ParseMatrixPacket(BinaryStream binaryStream)
        {
            var packetType = binaryStream.Read.String(4);
            Console.WriteLine($"Matrix packet type: {packetType}");

            switch (packetType)
            {
                case "POKE":
                    return new PokeMatrixPacket
                           {
                               SocketId = _socketId,
                               Type = Encoding.ASCII.GetBytes(packetType),
                               ProtocolVersion = binaryStream.Read.UShort()
                           };
                case "KISS":
                    return new KissMatrixPacket
                           {
                               SocketId = _socketId,
                               Type = Encoding.ASCII.GetBytes(packetType),
                               NextSocketId = binaryStream.Read.UInt(),
                               StreamingProtocolVersion = binaryStream.Read.UInt()
                           };
                case "ABRT":
                    return new AbrtMatrixPacket
                           {
                               SocketId = _socketId,
                               Type = Encoding.ASCII.GetBytes(packetType)
                           };
                default:
                    throw new UnknownMatrixPacketException(packetType);
            }
        }

        private GamePacket ParseGamePacket(BinaryStream binaryStream)
        {
            return new GamePacket
                   {
                       SocketId = _socketId
                   };
        }
    }
}
