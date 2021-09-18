using MatrixServer.Packets;
using Shared.Udp;
using System;
using System.Threading;

namespace MatrixServer
{
    internal class MatrixServer : PacketServer
    {
        protected Random random = new();

        public MatrixServer(ushort port) : base(port)
        {
        }

        protected override void HandlePacket(Packet packet, CancellationToken ct)
        {
            var mem = packet.PacketData;
            var SocketID = Utils.ReadStruct<uint>(mem);
            if (SocketID != 0)
            {
                return;
            }

            Program.Logger.Verbose("[MATRIX] " + packet.RemoteEndpoint + " sent " + packet.PacketData.Length + " bytes.");

            var matrixPkt = Utils.ReadStruct<MatrixPacketBase>(mem);

            switch (matrixPkt.Type)
            {
                case "POKE": // POKE
                    var poke = Utils.ReadStruct<MatrixPacketPoke>(mem);
                    Program.Logger.Verbose("[POKE]");
                    var socketID = GenerateSocketID();
                    Program.Logger.Information("Assigning SocketID [" + socketID + "] to [" + packet.RemoteEndpoint + "]");
                    _ = Send(Utils.WriteStruct(new MatrixPacketHehe(socketID)), packet.RemoteEndpoint);
                    break;
                case "KISS": // KISS
                    var kiss = Utils.ReadStruct<MatrixPacketKiss>(mem);
                    Program.Logger.Verbose("[KISS]");
                    _ = Send(Utils.WriteStruct(new MatrixPacketHugg(1, 25001)), packet.RemoteEndpoint);
                    break;
                case "ABRT": // ABRT
                    var abrt = Utils.ReadStruct<MatrixPacketAbrt>(mem);
                    Program.Logger.Verbose("[ABRT]");
                    break;
                default:
                    Program.Logger.Error("Unknown Matrix Packet Type: " + matrixPkt.Type);
                    return;
            }
        }

        protected uint GenerateSocketID()
        {
            return unchecked((uint)((0xff00ff << 8) | random.Next(0, 256)));
        }
    }
}